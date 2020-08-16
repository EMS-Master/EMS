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

        private float windProductionkW = 0;
        private float totalProduction = 0;

        public CalculationEngine()
        {
            publisher = new PublisherService();
            internalGenerators = new List<ResourceDescription>();
            internalEnergyConsumers = new List<ResourceDescription>();

            lockObj = new object();

            generators = new Dictionary<long, Generator>();
            energyConsumers = new Dictionary<long, EnergyConsumer>();
        }
        public bool Commit()
        {
            throw new NotImplementedException();
        }

        public bool Optimize(List<MeasurementUnit> measEnergyConsumer, List<MeasurementUnit> measGenerators, float windSpeed, float sunlight)
        {
            bool result = false;
            Dictionary<long, OptimisationModel> optModelMap = GetOptimizationModelMap(measGenerators, windSpeed, sunlight);
            float powerOfConsumers = CalculateConsumption(measEnergyConsumer);
            List<MeasurementUnit> measurementsOptimized = DoOptimization(optModelMap, powerOfConsumers, windSpeed, sunlight);
            totalProduction = 0;

            foreach (var m in measGenerators)
            {
                Console.WriteLine("masx value: " + m.MaxValue);
            }
            if (InsertMeasurementsIntoDb(measGenerators))
            {
                Console.WriteLine("Inserted {0} Measurement(s) into history database.", measGenerators.Count);
            }

			PublishGeneratorsToUI(measGenerators);

			try
            {
                if (measGenerators != null && measGenerators.Count > 0)
                {
                    totalProduction = measGenerators.Sum(x => x.CurrentValue);

                    if (WriteTotalProductionIntoDb(totalProduction, DateTime.Now))
                    {
                        Console.WriteLine("The total production is recorded into history database.");
                    }
                }
                if (ScadaCommandingProxy.Instance.SendDataToSimulator(measGenerators))
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
            lock (lockObj)
            {
                Dictionary<long, OptimisationModel> optModelMap = new Dictionary<long, OptimisationModel>();

                foreach (var measUnit in measGenerators)
                {
                    if (generators.ContainsKey(measUnit.Gid))
                    {
                        Generator g = generators[measUnit.Gid];
                        OptimisationModel om = new OptimisationModel(g, measUnit, windSpeed, sunlight);

                        optModelMap.Add(om.GlobalId, om);
                    }
                }

                return optModelMap;
            }
        }
        public float CalculateConsumption(IEnumerable<MeasurementUnit> measurements)
        {
            float retVal = 0;
            foreach (var item in measurements)
            {
                retVal += item.CurrentValue;
            }

            return retVal;
        }
        private List<MeasurementUnit> DoOptimization(Dictionary<long, OptimisationModel> optModelMap, float powerOfConsumers, float windSpeed, float sunlight)
        {
            try
            {
                Dictionary<long, OptimisationModel> optModelMapOptimizied = null;

                optModelMapOptimizied = CalculateWithGeneticAlgorithm(optModelMap, powerOfConsumers);
                return null;
            }
            catch (Exception e)
            {
                throw new Exception("[Method = DoOptimization] Exception = " + e.Message);
            }
        }
        private Dictionary<long, OptimisationModel> CalculateWithGeneticAlgorithm(Dictionary<long, OptimisationModel> optModelMap, float powerOfConsumers)
        {
            //Dictionary<long, OptimisationModel> optModelMapOptimizied;
            float powerOfConsumersWithoutRenewable = powerOfConsumers;

            Dictionary<long, OptimisationModel> optModelMapNonRenewable = new Dictionary<long, OptimisationModel>();
            foreach (var item in optModelMap)
            {
                if (item.Value.Renewable)
                {
                    item.Value.GenericOptimizedValue = item.Value.MaxPower;
                    powerOfConsumersWithoutRenewable -= item.Value.MaxPower;
                }
                else
                {
                    optModelMapNonRenewable.Add(item.Key, item.Value);
                }
            }
            float powerOfRenewable = powerOfConsumers - powerOfConsumersWithoutRenewable;
            
            return optModelMap;
        }
        public UpdateResult Prepare(ref Delta delta)
        {
            throw new NotImplementedException();
        }

        public bool Rollback()
        {
            throw new NotImplementedException();
        }

        public bool InsertMeasurementsIntoDb(List<MeasurementUnit> measurements)
        {
            bool success = true;
            try
            {
                using (var db = new EmsContext())
                {
                    foreach (var item in measurements)
                    {
                        HistoryMeasurement h = new HistoryMeasurement
                        {
                            Gid = item.Gid,
                            MeasurementTime = item.TimeStamp,
                            MeasurementValue = item.CurrentValue
                        };
                        db.HistoryMeasurements.Add(h);
                    }
                    db.SaveChanges();   
                }
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
                using (var db = new EmsContext())
                {
                    var dataFromDb = db.HistoryMeasurements.Where(x => x.Gid == gid && x.MeasurementTime >= startTime && x.MeasurementTime <= endTime).ToList();
                    foreach (var item in dataFromDb)
                    {
                        retVal.Add(new Tuple<double, DateTime>(item.MeasurementValue, item.MeasurementTime));
                    }
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

		private void PublishGeneratorsToUI(List<MeasurementUnit> measurementsFromGenerators)
		{
			List<MeasurementUI> measListUI = new List<MeasurementUI>();
			foreach (var meas in measurementsFromGenerators)
			{
				MeasurementUI measUI = new MeasurementUI();
				measUI.Gid = meas.Gid;
				measUI.CurrentValue = meas.CurrentValue;
				measUI.TimeStamp = meas.TimeStamp;
				//measUI.OptimizationType = 1;
				//measUI.Price = meas.CurrentPrice;
				measListUI.Add(measUI);
			}
			publisher.PublishOptimizationResults(measListUI);
		}

        public bool WriteTotalProductionIntoDb(float totalProduction, DateTime dateTime)
        {
            bool retVal = false;
            using (var db = new EmsContext())
            {
                try
                {
                    TotalProduction total = new TotalProduction() { TotalGeneration = totalProduction, TimeOfCalculation = dateTime };
                    db.TotalProductions.Add(total);
                    db.SaveChanges();

                    retVal = true;
                }
                catch (Exception e)
                {
                    retVal = false;
                    string message = string.Format("Failed to insert total production into database. {0}", e.Message);
                    CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                    Console.WriteLine(message);
                }
            }

            return retVal;
        }

        public List<Tuple<double, DateTime>> ReadTotalProductionsFromDb(DateTime startTime, DateTime endTime)
        {
            List<Tuple<double, DateTime>> retVal = new List<Tuple<double, DateTime>>();
            using (var db = new EmsContext())
            {
                try
                {
                    var list = db.TotalProductions.Where(x => x.TimeOfCalculation >= startTime && x.TimeOfCalculation <= endTime);
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
            }

            return retVal;
        }
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


    }
}
