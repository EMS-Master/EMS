using CalculationEngineServ.DataBaseModels;
using CalculationEngineServ.GeneticAlgorithm;
using CalculationEngineServ.PubSub;
using CommonMeas;
using FTN.Common;
using FTN.ServiceContracts;
using FTN.Services.NetworkModelService.DataModel.Wires;
using ScadaContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TransactionContract;

namespace CalculationEngineServ
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	public class CalculationEngine : ITransactionContract
    {
		private PublisherService publisher = null;
        private static List<ResourceDescription> internalGenerators;
        private static List<ResourceDescription> internalEnergyConsumers;
        private object lockObj;

        private static IDictionary<long, Generator> generators;
        private static IDictionary<long, EnergyConsumer> energyConsumers;
        private float previousEmissionCO2 = 0;
        private float currentEmissionCO2 = 0;
        private float totalCost = 0;

        private float windProductionkW = 0;
        private float windProductionPct = 0;
        private float totalProduction = 0;

        private static int ELITIMS_PERCENTAGE;
        private static int NUMBER_OF_ITERATION;
        private static int NUMBER_OF_POPULATION;
        private static float mutationRate;

        private float GenRenewable = 0;
        private float GenAll = 0;
        private float Diff = 0;

        public CalculationEngine()
        {
            SetAlgorithmParamsDefault();

            publisher = new PublisherService();
            internalGenerators = new List<ResourceDescription>();
            internalEnergyConsumers = new List<ResourceDescription>();

            lockObj = new object();

            generators = new Dictionary<long, Generator>();
            energyConsumers = new Dictionary<long, EnergyConsumer>();
        }
        

        public bool Optimize(List<MeasurementUnit> measEnergyConsumer, List<MeasurementUnit> measGenerators, float windSpeed, float sunlight)
        {
            bool result = false;
            Dictionary<long, OptimisationModel> optModelMap = GetOptimizationModelMap(measGenerators, windSpeed, sunlight);
            float powerOfConsumers = CalculateConsumption(measEnergyConsumer);

            float consMinusGenRen = powerOfConsumers - GenRenewable; 

            List<MeasurementUnit> measurementsOptimized = DoOptimization(optModelMap, powerOfConsumers, windSpeed, sunlight);
            totalProduction = 0;

           
            if (InsertMeasurementsIntoDb(measurementsOptimized))
            {
                Console.WriteLine("Inserted {0} Measurement(s) into history database.", measGenerators.Count);
            }

			PublishGeneratorsToUI(measurementsOptimized);
            PublishConsumersToUI(measEnergyConsumer);


            try
            {
                if (measurementsOptimized != null && measurementsOptimized.Count > 0)
                {
                    totalProduction = measurementsOptimized.Sum(x => x.CurrentValue);

                    if (WriteTotalProductionIntoDb(totalProduction, totalCost, DateTime.Now))
                    {
                        Console.WriteLine("The total production is recorded into history database.");
                    }
                    if (WriteCO2EmissionIntoDb(previousEmissionCO2, currentEmissionCO2))
                    {
                        Console.WriteLine("The CO2 emission is recorded into history database.");
                    }
                }
                if (ScadaCommandingProxy.Instance.SendDataToSimulator(measurementsOptimized))
                {
                    CommonTrace.WriteTrace(CommonTrace.TraceInfo, "CE sent {0} optimized MeasurementUnit(s) to SCADACommanding.", measGenerators.Count);
                    Console.WriteLine("CE sent {0} optimized MeasurementUnit(s) to SCADACommanding.", measGenerators.Count);

                    result = true;
                }
                // ScadaCommandingProxy.Instance.CommandDiscreteValues(25769803777,true);
            }
            catch (Exception ex)
            {
                CommonTrace.WriteTrace(CommonTrace.TraceError, ex.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, ex.StackTrace);
            }
            return result;
        }
        private Dictionary<long, OptimisationModel> GetOptimizationModelMap(List<MeasurementUnit> measGenerators, float windSpeed, float sunlight)
        {
            float sumInGetOptModelMap = 0;
            float wholeSum = 0;
            //lock (lockObj)
            //{
                Dictionary<long, OptimisationModel> optModelMap = new Dictionary<long, OptimisationModel>();

                foreach (var measUnit in measGenerators)
                {
                    if (generators.ContainsKey(measUnit.Gid))
                    {
                        Generator g = generators[measUnit.Gid];
                        wholeSum += measUnit.CurrentValue;
                        if (g.GeneratorType == GeneratorType.Hydro || g.GeneratorType == GeneratorType.Solar || g.GeneratorType == GeneratorType.Wind)
                            sumInGetOptModelMap += measUnit.CurrentValue;
                        OptimisationModel om = new OptimisationModel(g, measUnit, windSpeed, sunlight);

                        optModelMap.Add(om.GlobalId, om);
                    }
                }

                GenAll = wholeSum;
                GenRenewable = sumInGetOptModelMap;
                Diff = wholeSum - sumInGetOptModelMap;

                Console.WriteLine("WholeSUm: " + wholeSum);
                Console.WriteLine("sumInGetOptModelMap: " + sumInGetOptModelMap);
                Console.WriteLine("Difference: " + (wholeSum - sumInGetOptModelMap));

                return optModelMap;
            //}
        }
        
        private List<MeasurementUnit> DoOptimization(Dictionary<long, OptimisationModel> optModelMap, float powerOfConsumers, float windSpeed, float sunlight)
        {
          
                Dictionary<long, OptimisationModel> optModelMapOptimizied = null;

                optModelMapOptimizied = CalculateWithGeneticAlgorithm(optModelMap, powerOfConsumers);
				return optModelMapOptimizied.Select(x => x.Value.measurementUnit).ToList();
           
        }
        private Dictionary<long, OptimisationModel> CalculateWithGeneticAlgorithm(Dictionary<long, OptimisationModel> optModelMap, float powerOfConsumers)
        {
            Dictionary<long, OptimisationModel> optModelMapOptimizied;
            float powerOfConsumersWithoutRenewable = powerOfConsumers;

            Dictionary<long, OptimisationModel> optModelMapNonRenewable = new Dictionary<long, OptimisationModel>();
            foreach (var item in optModelMap)
            {
                if (item.Value.Renewable)
                {
                    item.Value.GenericOptimizedValue = item.Value.MeasuredValue;
                    powerOfConsumersWithoutRenewable -= item.Value.MeasuredValue;
                    if(item.Value.TypeGenerator == GeneratorType.Wind)
                    {
                        windProductionkW += item.Value.MeasuredValue;
                    }
                }
                else
                {
                    optModelMapNonRenewable.Add(item.Key, item.Value);
                }
            }
            float powerOfRenewable = powerOfConsumers - powerOfConsumersWithoutRenewable;

            windProductionPct = windProductionkW / powerOfConsumers * 100;

            previousEmissionCO2 = CalculateCO2(optModelMapNonRenewable);
            var previousCost = CalculateCost(optModelMapNonRenewable); 

            GA gaoRenewable = new GA(powerOfConsumersWithoutRenewable, optModelMapNonRenewable);
            optModelMapOptimizied = gaoRenewable.StartAlgorithm(NUMBER_OF_ITERATION,NUMBER_OF_POPULATION,ELITIMS_PERCENTAGE,mutationRate);
            // calculate power of each

            //new
            totalCost= gaoRenewable.TotalCost;
            currentEmissionCO2 = gaoRenewable.EmissionCO2;

            foreach(var item in optModelMapOptimizied)
            {
                if (optModelMap.ContainsKey(item.Key))
                    optModelMap[item.Key] = item.Value;
            }

            return optModelMap;
        }

        #region Transaction
        public UpdateResult Prepare(ref Delta delta)
        {
            throw new NotImplementedException();
        }
        public bool Commit()
        {
            throw new NotImplementedException();
        }
        public bool Rollback()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region DataBase

        public bool InsertMeasurementsIntoDb(List<MeasurementUnit> measurements)
        {
            bool success = true;
            try
            {
                
                foreach (var item in measurements)
                {
                    HistoryMeasurement h = new HistoryMeasurement
                    {
                        Gid = item.Gid,
                        MeasurementTime = item.TimeStamp,
                        MeasurementValue = item.CurrentValue
                    };
                    DbManager.Instance.AddHistoryMeasurement(h);
                }
                DbManager.Instance.SaveChanges();   
                
            }
            catch (Exception e)
            {
                success = false;
                string message = string.Format("Failed to insert new Measurement into database. {0}", e.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                Console.WriteLine(message);
            }

            return success;
        }

        public List<Tuple<double, DateTime>> ReadMeasurementsFromDb(long gid, DateTime startTime, DateTime endTime)
        {
            List<Tuple<double, DateTime>> retVal = new List<Tuple<double, DateTime>>();
            try
            {
                 var dataFromDb = DbManager.Instance.GetHistoryMeasurements().Where(x => x.Gid == gid && x.MeasurementTime >= startTime && x.MeasurementTime <= endTime).ToList();
                foreach (var item in dataFromDb)
                {
                    retVal.Add(new Tuple<double, DateTime>(item.MeasurementValue, item.MeasurementTime));
                }
                
            }
            catch (Exception e)
            {
                string message = string.Format("Failed read Measurements from database. {0}", e.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                Console.WriteLine(message);
            }

            return retVal;
        }

        public bool WriteTotalProductionIntoDb(float totalProduction, float totalCost, DateTime dateTime)
        {
            bool retVal = false;
            
            try
            {
                TotalProduction total = new TotalProduction() { TotalGeneration = totalProduction, TimeOfCalculation = dateTime, TotalCost = totalCost };
                DbManager.Instance.AddTotalProduction(total);
                DbManager.Instance.SaveChanges();

                retVal = true;
            }
            catch (Exception e)
            {
                retVal = false;
                string message = string.Format("Failed to insert total production into database. {0}", e.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                Console.WriteLine(message);
            }
            

            return retVal;
        }

        public bool WriteCO2EmissionIntoDb(float previousEmissionCO2, float currentEmissionCO2)
        {
            bool retVal = false;
            
            try
            {
                CO2Emission total = new CO2Emission() { PreviousEmission = previousEmissionCO2, CurrentEmission = currentEmissionCO2, Timestamp = DateTime.Now };
                DbManager.Instance.AddCO2Emission(total);
                DbManager.Instance.SaveChanges();

                retVal = true;
            }
            catch (Exception e)
            {
                retVal = false;
                string message = string.Format("Failed to insert CO2 emission into database. {0}", e.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                Console.WriteLine(message);
            }
            

            return retVal;
        }

        public List<Tuple<double, DateTime>> ReadTotalProductionsFromDb(DateTime startTime, DateTime endTime)
        {
            List<Tuple<double, DateTime>> retVal = new List<Tuple<double, DateTime>>();
            
            try
            {
                var list = DbManager.Instance.GetTotalProductions().Where(x => x.TimeOfCalculation >= startTime && x.TimeOfCalculation <= endTime).ToList();
                foreach (var item in list)
                {
                    retVal.Add(new Tuple<double, DateTime>(item.TotalGeneration, item.TimeOfCalculation));
                }
            }
            catch (Exception e)
            {
                string message = string.Format("Failed read Measurements from database. {0}", e.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                Console.WriteLine(message);
            }
            

            return retVal;
        }

        #endregion

        #region Publish to UI

        private void PublishGeneratorsToUI(List<MeasurementUnit> measurementsFromGenerators)
		{
			List<MeasurementUI> measListUI = new List<MeasurementUI>();
			foreach (var gens in generators)
			{
                if(measurementsFromGenerators.Any(x => x.Gid== gens.Key))
                {
                    var k = measurementsFromGenerators.Where(x => x.Gid == gens.Key).FirstOrDefault();
                    MeasurementUI measUI = new MeasurementUI();
                    measUI.Gid = gens.Key;
                    measUI.CurrentValue = k.CurrentValue/1000;
                    measUI.TimeStamp = k.TimeStamp;
                    measUI.IsActive = true;
                    measUI.GeneratorType = gens.Value.GeneratorType;
                    measUI.Name = gens.Value.Name;
                    measListUI.Add(measUI);
                }
                else
                {
                    MeasurementUI measUI = new MeasurementUI();
                    measUI.Gid = gens.Key;
                    measUI.CurrentValue = 0;
                    measUI.TimeStamp = DateTime.Now;
                    measUI.IsActive = false;
                    measUI.GeneratorType = gens.Value.GeneratorType;
                    measUI.Name = gens.Value.Name;
                    measListUI.Add(measUI);
                }
				
			}
			publisher.PublishOptimizationResults(measListUI);
		}

        private void PublishConsumersToUI(List<MeasurementUnit> measurementsFromConsumers)
        {
            List<MeasurementUI> measUIList = new List<MeasurementUI>();
            foreach (var meas in energyConsumers)
            {
                if (measurementsFromConsumers.Any(x => x.Gid == meas.Key))
                {
                    var k = measurementsFromConsumers.Where(x => x.Gid == meas.Key).FirstOrDefault();
                    MeasurementUI measUI = new MeasurementUI();
                    measUI.Gid = k.Gid;
                    measUI.CurrentValue = k.CurrentValue/1000;
                    measUI.GeneratorType = GeneratorType.Unknown;
                    measUI.Name = meas.Value.Name;
                    measUI.TimeStamp = k.TimeStamp;
                    measUIList.Add(measUI);
                }
                else
                {
                    MeasurementUI measUI = new MeasurementUI();
                    measUI.Gid = meas.Key;
                    measUI.CurrentValue = 0;
                    measUI.TimeStamp = DateTime.Now;
                    measUI.IsActive = false;
                    measUI.GeneratorType = GeneratorType.Unknown;
                    measUI.Name = meas.Value.Name;
                    measUIList.Add(measUI);
                }
            }
            publisher.PublishOptimizationResults(measUIList);
        }

        #endregion

        public bool InitiateIntegrityUpdate()
        {
            //lock (lockObj)
            {
                ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();

                List<ModelCode> properties = new List<ModelCode>(20);
                List<ResourceDescription> retList = new List<ResourceDescription>(5);

                int iteratorId = 0;
                int resourcesLeft = 0;
                int numberOfResources = 2;
                string message = string.Empty;

                #region getting Generators

                try
                {
                    // get all generators from NMS
                    properties = modelResourcesDesc.GetAllPropertyIds(ModelCode.GENERATOR);

                    iteratorId = NetworkModelGDAProxy.Instance.GetExtentValues(ModelCode.GENERATOR, properties);
                    resourcesLeft = NetworkModelGDAProxy.Instance.IteratorResourcesLeft(iteratorId);

                    while (resourcesLeft > 0)
                    {
                        List<ResourceDescription> rds = NetworkModelGDAProxy.Instance.IteratorNext(numberOfResources, iteratorId);
                        retList.AddRange(rds);
                        resourcesLeft = NetworkModelGDAProxy.Instance.IteratorResourcesLeft(iteratorId);
                    }
                    NetworkModelGDAProxy.Instance.IteratorClose(iteratorId);

                    // add synchronous machines to internal collection
                    internalGenerators.Clear();
                    internalGenerators.AddRange(retList);
                }
                catch (Exception e)
                {
                    message = string.Format("Getting extent values method failed for {0}.\n\t{1}", ModelCode.GENERATOR, e.Message);
                    Console.WriteLine(message);
                    CommonTrace.WriteTrace(CommonTrace.TraceError, message);

                    Console.WriteLine("Trying again...");
                    CommonTrace.WriteTrace(CommonTrace.TraceError, "Trying again...");
                    NetworkModelGDAProxy.Instance = null;
                    Thread.Sleep(1000);
                    InitiateIntegrityUpdate();
                }

                message = string.Format("Integrity update: Number of {0} values: {1}", ModelCode.GENERATOR.ToString(), internalGenerators.Count.ToString());
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
                Console.WriteLine("Integrity update: Number of {0} values: {1}", ModelCode.GENERATOR.ToString(), internalGenerators.Count.ToString());

                // clear retList for getting new model from NMS
                retList.Clear();

                properties.Clear();
                iteratorId = 0;
                resourcesLeft = 0;

                #endregion getting Generators

                #region getting EnergyConsumer

                try
                {
                    // third get all enenrgy consumers from NMS
                    properties = modelResourcesDesc.GetAllPropertyIds(ModelCode.ENERGY_CONSUMER);

                    iteratorId = NetworkModelGDAProxy.Instance.GetExtentValues(ModelCode.ENERGY_CONSUMER, properties);
                    resourcesLeft = NetworkModelGDAProxy.Instance.IteratorResourcesLeft(iteratorId);

                    while (resourcesLeft > 0)
                    {
                        List<ResourceDescription> rds = NetworkModelGDAProxy.Instance.IteratorNext(numberOfResources, iteratorId);
                        retList.AddRange(rds);
                        resourcesLeft = NetworkModelGDAProxy.Instance.IteratorResourcesLeft(iteratorId);
                    }
                    NetworkModelGDAProxy.Instance.IteratorClose(iteratorId);

                    // add energy consumer to internal collection
                    internalEnergyConsumers.Clear();
                    internalEnergyConsumers.AddRange(retList);
                }
                catch (Exception e)
                {
                    message = string.Format("Getting extent values method failed for {0}.\n\t{1}", ModelCode.ENERGY_CONSUMER, e.Message);
                    Console.WriteLine(message);
                    CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                    return false;
                }

                message = string.Format("Integrity update: Number of {0} values: {1}", ModelCode.ENERGY_CONSUMER.ToString(), internalEnergyConsumers.Count.ToString());
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
                Console.WriteLine("Integrity update: Number of {0} values: {1}", ModelCode.ENERGY_CONSUMER.ToString(), internalEnergyConsumers.Count.ToString());

                // clear retList
                retList.Clear();

                #endregion getting EnergyConsumer

                FillData();
                return true;
            }

        }

        private void FillData()
        {
            lock (lockObj)
            {
                generators.Clear();
                energyConsumers.Clear();

                foreach (ResourceDescription rd in internalGenerators)
                {
                    Generator g = ResourcesDescriptionConverter.ConvertTo<Generator>(rd);
                    generators.Add(g.GlobalId, g);
                }
                foreach (ResourceDescription rd in internalEnergyConsumers)
                {
                    EnergyConsumer ec = ResourcesDescriptionConverter.ConvertTo<EnergyConsumer>(rd);
                    energyConsumers.Add(ec.GlobalId, ec);
                }
            }
        }

        private static float CalculateCost(Dictionary<long, OptimisationModel> optModelMap)
        {
            float cost = 0;
            foreach (var optModel in optModelMap.Values)
            {
                float price = optModel.CalculatePrice(optModel.GenericOptimizedValue);
                cost += price;
            }

            return cost;
        }

        public static float CalculateCO2(Dictionary<long, OptimisationModel> optModelMap)
        {
            //CO2 Emissions from each fuel (tonnes) = Energy consumption of fuel (kWh) x Emission factor for each fuel (kgCO2/kWh) x 0.001
            float emCO2 = 0;
            foreach (var optModel in optModelMap.Values)
            {
                emCO2 += optModel.GenericOptimizedValue * optModel.EmissionFactor * 0.001f;
            }
            return emCO2;
        }

        private float CalculateConsumption(IEnumerable<MeasurementUnit> measurements)
        {
            float retVal = 0;
            foreach (var item in measurements)
            {
                retVal += item.CurrentValue;
            }

            return retVal;
        }

        public static bool SetAlgorithmParams(int iterationCount, int populationCount, int elitisamPct, float mutationRat)
        {
            ELITIMS_PERCENTAGE = elitisamPct;
            NUMBER_OF_ITERATION = iterationCount;
            NUMBER_OF_POPULATION = populationCount;
            mutationRate = mutationRat;
            return true;
        }

        public static bool SetAlgorithmParamsDefault()
        {
            ELITIMS_PERCENTAGE = 5;
            NUMBER_OF_ITERATION = 200;
            NUMBER_OF_POPULATION = 100;
            mutationRate = 1f;
            return true;
        }

        public static Tuple<int, int, int, float> GetAlgorithmParams()
        {
            return new Tuple<int, int, int, float>(NUMBER_OF_ITERATION, NUMBER_OF_POPULATION, ELITIMS_PERCENTAGE, mutationRate);
        }
    }
}
