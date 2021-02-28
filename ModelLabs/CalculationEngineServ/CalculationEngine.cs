
using CalculationEngineContracts.ServiceFabricProxy;
using CalculationEngineServ.GeneticAlgorithm;
using CalculationEngineServ.PubSub;
using CEPubSubContract;
using CommonCloud.AzureStorage.Entities;
using CommonCloud.AzureStorage.Helpers;
using CommonMeas;
using FTN.Common;
using FTN.ServiceContracts;
using FTN.ServiceContracts.ServiceFabricProxy;
using FTN.Services.NetworkModelService.DataModel.Wires;
using ScadaContracts;
using ScadaContracts.ServiceFabricProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using TransactionContract;

namespace CalculationEngineServ
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	public class CalculationEngine : ITransactionContract
    {
		private PublisherService publisher = null;
        private CEPublishProxy CePublishProxy = new CEPublishProxy();
        private static List<ResourceDescription> internalGenerators;
        private static List<ResourceDescription> internalGeneratorsCopy;
        private static List<ResourceDescription> internalEnergyConsumers;
        private static List<ResourceDescription> internalEnergyConsumersCopy;
        private object lockObj;

        private static IDictionary<long, Generator> generators;
        private static IDictionary<long, EnergyConsumer> energyConsumers;
        private float reductionCO2 = 0;
        private float currentEmissionCO2 = 0;
        private float totalCost = 0;
		private float profit = 0;

		private float windProductionkW = 0;
        private float windProductionPct = 0;
        private float totalProduction = 0;
		private float renewableConributionKW = 0;
		private float renewableContributionPrct = 0;

        private static int ELITIMS_PERCENTAGE;
        private static int NUMBER_OF_ITERATION;
        private static int NUMBER_OF_POPULATION;
        private static float mutationRate;

        private float GenRenewable = 0;
        private float GenAll = 0;
        private float Diff = 0;

        private ITransactionCallback transactionCallback;
        private UpdateResult updateResult;
		private static Dictionary<GeneratorType, float> allTypes;
		private static Dictionary<long, OptimisationModel> optimizationModelResults;

		public static List<GeneratorCurveModel> generatorCurves;
        public static Dictionary<Tuple<long, string>, int> MaxDiscreteCounter;
        //private CeRepositoryManagerSfProxy ceRepoProxy;
		
		public CalculationEngine()
        {
            SetAlgorithmParamsDefault();
			SetPricePerGeneratorTypeDefault();

            //ceRepoProxy = new CeRepositoryManagerSfProxy("CeRepositoryManagerEndpoint");

			publisher = new PublisherService();
            internalGenerators = new List<ResourceDescription>();
            internalGeneratorsCopy = new List<ResourceDescription>();
            internalEnergyConsumers = new List<ResourceDescription>();
            internalEnergyConsumersCopy = new List<ResourceDescription>();

            lockObj = new object();

            generators = new Dictionary<long, Generator>();
            energyConsumers = new Dictionary<long, EnergyConsumer>();
			generatorCurves = LoadXMLFile.Load().Curves;
			optimizationModelResults = new Dictionary<long, OptimisationModel>();
            MaxDiscreteCounter = new Dictionary<Tuple<long, string>, int>();
        }
        

        public bool Optimize(List<MeasurementUnit> measEnergyConsumer, List<MeasurementUnit> measGenerators, float windSpeed, float sunlight)
        {
            bool result = false;
            Dictionary<long, OptimisationModel> optModelMap = GetOptimizationModelMap(measGenerators, windSpeed, sunlight);
            float powerOfConsumers = CalculateConsumption(measEnergyConsumer);

            float consMinusGenRen = powerOfConsumers - GenRenewable;
			List<MeasurementUnit> measurementsOptimized = optModelMap.Select(x => x.Value.measurementUnit).ToList();
			if (optModelMap.Count>13)
				measurementsOptimized = DoOptimization(optModelMap, powerOfConsumers, windSpeed, sunlight);


			totalProduction = 0;
			
            if (InsertMeasurementsIntoDb(measurementsOptimized))
            {
                Console.WriteLine("Inserted {0} Measurement(s) into history database.", measGenerators.Count);
            }

			PublishGeneratorsToUI(measurementsOptimized);
            PublishConsumersToUI(measEnergyConsumer);

            PublishRenewableToUI(new Tuple<DateTime, float>(DateTime.Now, renewableConributionKW));
            PublishCoReductionToUI(new Tuple<string, float, float>("coReduction", reductionCO2, currentEmissionCO2));
            PublishCoReductionToUI(new Tuple<string, float, float>("cost", totalCost, profit));


            PublishWindPercent(renewableContributionPrct);//(windProductionPct);
			
            try
            {
                if (measurementsOptimized != null && measurementsOptimized.Count > 0)
                {
                    totalProduction = measurementsOptimized.Sum(x => x.CurrentValue);

                    if (WriteTotalProductionIntoDb(totalProduction, totalCost, DateTime.Now))
                    {
                        Console.WriteLine("The total production is recorded into history database.");
                    }
                }
                try
                {
                    ScadaCommandingSfProxy scadaSf = new ScadaCommandingSfProxy();
                    if (scadaSf.SendDataToSimulator(measurementsOptimized))
                    {
                        CommonTrace.WriteTrace(CommonTrace.TraceInfo, "CE sent {0} optimized MeasurementUnit(s) to SCADACommanding.", measGenerators.Count);
                        Console.WriteLine("CE sent {0} optimized MeasurementUnit(s) to SCADACommanding.", measGenerators.Count);

                        result = true;
                    }
                }
                catch (Exception ex)
                {
                    CommonTrace.WriteTrace(CommonTrace.TraceError, ex.Message);
                    CommonTrace.WriteTrace(CommonTrace.TraceError, ex.StackTrace);
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
					OptimisationModel om = null;

					if (generators.ContainsKey(measUnit.Gid))
                    {
                        Generator g = generators[measUnit.Gid];

						wholeSum += measUnit.CurrentValue;
						if (g.GeneratorType == GeneratorType.Hydro || g.GeneratorType == GeneratorType.Solar || g.GeneratorType == GeneratorType.Wind)
							sumInGetOptModelMap += measUnit.CurrentValue;
					
						float percentage = (100 * (measUnit.CurrentValue)) / (g.MaxQ);

						GeneratorCurveModel generatorCurveModel= generatorCurves.FirstOrDefault(x => x.LowerPoint <= percentage && x.HigherPoint >= percentage && x.GeneratorType.Split('_')[0] == g.GeneratorType.ToString());
						om = new OptimisationModel(g, measUnit, windSpeed, sunlight, generatorCurveModel);
					
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
			//Dictionary<long, OptimisationModel> optModelMapOptimizied = null;
			optimizationModelResults = new Dictionary<long, OptimisationModel>();

			optimizationModelResults = CalculateWithGeneticAlgorithm(optModelMap, powerOfConsumers);
			return optimizationModelResults.Select(x => x.Value.measurementUnit).ToList();
           
        }
        private Dictionary<long, OptimisationModel> CalculateWithGeneticAlgorithm(Dictionary<long, OptimisationModel> optModelMap, float powerOfConsumers)
        {
            CeRepositoryManagerSfProxy ceRepoProxy = new CeRepositoryManagerSfProxy();
            Dictionary<long, OptimisationModel> optModelMapOptimizied = null;
            float powerOfConsumersWithoutRenewable = powerOfConsumers;

            Dictionary<long, OptimisationModel> optModelMapNonRenewable = new Dictionary<long, OptimisationModel>();
			Dictionary<long, OptimisationModel> renewableGenerators = new Dictionary<long, OptimisationModel>();

			renewableConributionKW = optModelMap.Where(x => x.Value.Renewable).Select(y => y.Value.MeasuredValue).Sum();
			List<long> generatorsCommandedFromUI = ceRepoProxy.GetGidsForCommandedGenerators();
			foreach (var item in optModelMap)
            {
                if (item.Value.Renewable)
                {
                    item.Value.GenericOptimizedValue = item.Value.MeasuredValue;
                    powerOfConsumersWithoutRenewable -= item.Value.MeasuredValue;
					item.Value.measurementUnit.CurrentValue = item.Value.MeasuredValue;
                    if(item.Value.TypeGenerator == GeneratorType.Wind)
                    {
                        windProductionkW += item.Value.MeasuredValue;
                    }
					renewableGenerators.Add(item.Key,item.Value);
                }
                else
                {
					optModelMapNonRenewable.Add(item.Key, item.Value);
                }
            }
			
            float powerOfRenewable = powerOfConsumers - powerOfConsumersWithoutRenewable;
			
            windProductionPct = (windProductionkW * 100) / powerOfConsumers;
			windProductionkW = 0;

			bool isNecessaryEnergyZero = false;
			GA gaoRenewable = new GA(powerOfConsumersWithoutRenewable, optModelMapNonRenewable);
			if (gaoRenewable.NecessaryEnergy > 0)
			{
				optModelMapOptimizied = gaoRenewable.StartAlgorithm(NUMBER_OF_ITERATION, NUMBER_OF_POPULATION, ELITIMS_PERCENTAGE, mutationRate);

				foreach (var item in optModelMapOptimizied)
				{
					if (optModelMap.ContainsKey(item.Key))
						optModelMap[item.Key] = item.Value;
				}
			}
			else
			{
				Dictionary<long, OptimisationModel> commandedValues = new Dictionary<long, OptimisationModel>();
				foreach (var item in optModelMapNonRenewable)
				{
					if (gaoRenewable.CommandedGenGidsAndValues.ContainsKey(item.Key))
					{
						item.Value.GenericOptimizedValue = gaoRenewable.CommandedGenGidsAndValues[item.Key];
						item.Value.MeasuredValue = gaoRenewable.CommandedGenGidsAndValues[item.Key];
						item.Value.measurementUnit.CurrentValue = gaoRenewable.CommandedGenGidsAndValues[item.Key];
						optModelMap[item.Key] = item.Value;
						commandedValues.Add(item.Key, item.Value);
					}
					else if (optModelMap.ContainsKey(item.Key))
					{
						item.Value.GenericOptimizedValue = 0;
						item.Value.MeasuredValue = 0;
						item.Value.measurementUnit.CurrentValue = 0;
						optModelMap[item.Key] = item.Value;
					}
				}
				isNecessaryEnergyZero = true;
				CalculateTotalCostWhenNecessaryEnergyIsZero(commandedValues);
			}
			
			//reductionCO2 = CalculateCO2WithKyotoProtocol(renewableGenerators);
			reductionCO2 = CalculateCO2ReductionWithBiggestCoeficient(renewableGenerators);
			renewableContributionPrct = (renewableConributionKW * 100) / powerOfConsumers;
			totalCost = isNecessaryEnergyZero ? totalCost : gaoRenewable.TotalCost;
			currentEmissionCO2 = CalculateCO2(optModelMap);
			profit = GetProfit(optModelMap);

			return optModelMap;
        }
		
		private void CalculateTotalCostWhenNecessaryEnergyIsZero(Dictionary<long, OptimisationModel> optModelMap)
		{
			foreach (var item in optModelMap)
			{
				float price = item.Value.CalculatePrice(item.Value.GenericOptimizedValue);
				item.Value.Price = price;
				totalCost += price;
			}
		}

        #region Transaction
        public UpdateResult Prepare(ref Delta delta)
        {
            try
            {
                transactionCallback = OperationContext.Current.GetCallbackChannel<ITransactionCallback>();
                updateResult = new UpdateResult();
                
                internalGeneratorsCopy.Clear();
                foreach (ResourceDescription rd in internalGenerators)
                {
                    internalGeneratorsCopy.Add(rd.Clone() as ResourceDescription);
                }

                internalEnergyConsumersCopy.Clear();
                foreach (ResourceDescription rd in internalEnergyConsumers)
                {
                    internalEnergyConsumersCopy.Add(rd.Clone() as ResourceDescription);
                }

                foreach (ResourceDescription rd in delta.InsertOperations)
                {
                    foreach (Property prop in rd.Properties)
                    {
                        if (ModelCodeHelper.GetTypeFromModelCode(prop.Id).Equals(DMSType.GENERATOR))
                        {
                            internalGeneratorsCopy.Add(rd);
                            break;
                        }
                        else if (ModelCodeHelper.GetTypeFromModelCode(prop.Id).Equals(DMSType.ENERGY_CONSUMER))
                        {
                            internalEnergyConsumersCopy.Add(rd);
                            break;
                        }
                    }
                }

                foreach (ResourceDescription rd in delta.UpdateOperations)
                {
                    foreach (Property prop in rd.Properties)
                    {
                        if ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(rd.Id) == (DMSType.GENERATOR))
                        {
                            foreach (ResourceDescription res in internalGeneratorsCopy)
                            {
                                if (rd.Id.Equals(res.Id))
                                {
                                    foreach (Property p in res.Properties)
                                    {
                                        if (prop.Id.Equals(p.Id))
                                        {
                                            p.PropertyValue = prop.PropertyValue;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else if ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(rd.Id) == (DMSType.ENERGY_CONSUMER))
                        {
                            foreach (ResourceDescription res in internalEnergyConsumersCopy)
                            {
                                if (rd.Id.Equals(res.Id))
                                {
                                    foreach (Property p in res.Properties)
                                    {
                                        if (prop.Id.Equals(p.Id))
                                        {
                                            p.PropertyValue = prop.PropertyValue;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                updateResult.Message = "CE Transaction Prepare finished.";
                updateResult.Result = ResultType.Succeeded;
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, "CETransaction Prepare finished successfully.");
                transactionCallback.Response("OK");
            }
            catch (Exception e)
            {
                updateResult.Message = "CE Transaction Prepare finished.";
                updateResult.Result = ResultType.Failed;
                CommonTrace.WriteTrace(CommonTrace.TraceWarning, "CE Transaction Prepare failed. Message: {0}", e.Message);
                transactionCallback.Response("ERROR");
            }

            return updateResult;
        }
        public bool Commit()
        {
            try
            {
                internalGenerators.Clear();
                foreach (ResourceDescription rd in internalGeneratorsCopy)
                {
                    internalGenerators.Add(rd.Clone() as ResourceDescription);
                }
                internalGeneratorsCopy.Clear();

                
                internalEnergyConsumers.Clear();
                foreach (ResourceDescription rd in internalEnergyConsumersCopy)
                {
                    internalEnergyConsumers.Add(rd.Clone() as ResourceDescription);
                }
                internalEnergyConsumersCopy.Clear();

                CommonTrace.WriteTrace(CommonTrace.TraceInfo, "CE Transaction: Commit phase successfully finished.");
                Console.WriteLine("Number of SynchronousMachines values: {0}", internalGenerators.Count);
                Console.WriteLine("Number of Energy Consumers values: {0}", internalEnergyConsumers.Count);

                FillData();
                return true;
            }
            catch (Exception e)
            {
                CommonTrace.WriteTrace(CommonTrace.TraceWarning, "CE Transaction: Failed to Commit changes. Message: {0}", e.Message);
                return false;
            }
        }
        public bool Rollback()
        {
            try
            {
                internalGeneratorsCopy.Clear();
                internalEnergyConsumersCopy.Clear();
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, "CE Transaction rollback successfully finished!");
                return true;
            }
            catch (Exception e)
            {
                CommonTrace.WriteTrace(CommonTrace.TraceError, "CE Transaction rollback error. Message: {0}", e.Message);
                return false;
            }
        }
        #endregion

        #region DataBase

        public bool InsertMeasurementsIntoDb(List<MeasurementUnit> measurements)
        {
            CeRepositoryManagerSfProxy ceRepoProxy = new CeRepositoryManagerSfProxy();
            bool success = true;
            try
            {
                
                foreach (var item in measurements)
                {
                    HistoryMeasurementHelper h = new HistoryMeasurementHelper(item.Gid, item.TimeStamp,item.CurrentValue);
                   
                    ceRepoProxy.AddHistoryMeasurement(h);
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
            CeRepositoryManagerSfProxy ceRepoProxy = new CeRepositoryManagerSfProxy();

            List<Tuple<double, DateTime>> retVal = new List<Tuple<double, DateTime>>();
            try
            {
                var dataFromDb = ceRepoProxy.GetAllHistoryMeasurementsForSelectedTime(startTime, endTime, gid);
                foreach (var item in dataFromDb)
                {
                    retVal.Add(new Tuple<double, DateTime>((double)item.MeasurementValue, item.MeasurementTime));
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
            CeRepositoryManagerSfProxy ceRepoProxy = new CeRepositoryManagerSfProxy();

            bool retVal = false;
            
            try
            {
                TotalProductionHelper total = new TotalProductionHelper()
				{
					TotalGeneration = totalProduction,
					CO2Reduction = reductionCO2,
					CO2Emission = currentEmissionCO2,
					TimeOfCalculation = dateTime,
					TotalCost = totalCost,
					Profit = profit,
                    
            };

                ceRepoProxy.AddTotalProduction(total);

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
		
        //public List<Tuple<double, DateTime>> ReadTotalProductionsFromDb(DateTime startTime, DateTime endTime)
        //{
        //    List<Tuple<double, DateTime>> retVal = new List<Tuple<double, DateTime>>();
            
        //    try
        //    {
        //        var list = DbManager.Instance.GetTotalProductionsForSelectedTime(startTime, endTime);
        //        foreach (var item in list)
        //        {
        //            retVal.Add(new Tuple<double, DateTime>((double)item.TotalGeneration, item.TimeOfCalculation.ToLocalTime()));
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        string message = string.Format("Failed read Measurements from database. {0}", e.Message);
        //        CommonTrace.WriteTrace(CommonTrace.TraceError, message);
        //        Console.WriteLine(message);
        //    }
            

        //    return retVal;
        //}

        public List<Tuple<DateTime, double, double, double, double, double>> ReadTotalProductions(DateTime startTime, DateTime endTime)
        {
            CeRepositoryManagerSfProxy ceRepoProxy = new CeRepositoryManagerSfProxy();

            var retList = new List<Tuple<DateTime, double, double, double, double, double>>();
            try
            {
                var list = ceRepoProxy.GetTotalProductionsForSelectedTime(startTime, endTime);
                foreach(var item in list)
                {
                    retList.Add(new Tuple<DateTime, double, double, double, double, double>(item.TimeOfCalculation, item.TotalGeneration, item.Profit, item.TotalCost, item.CO2Emission, item.CO2Reduction));
                }
            }
            catch (Exception e)
            {
                string message = string.Format("Failed read Measurements from database. {0}", e.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                Console.WriteLine(message);
            }

            return retList;
        }

        public List<Tuple<double, DateTime>> ReadTotalProfitFromDb(DateTime startTime, DateTime endTime)
        {
            CeRepositoryManagerSfProxy ceRepoProxy = new CeRepositoryManagerSfProxy();

            List<Tuple<double, DateTime>> retVal = new List<Tuple<double, DateTime>>();

            try
            {
                var list = ceRepoProxy.GetTotalProductionsForSelectedTime(startTime, endTime);
                foreach (var item in list)
                {
                    retVal.Add(new Tuple<double, DateTime>((double)item.Profit, item.TimeOfCalculation.ToLocalTime()));
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

        public List<Tuple<double, DateTime>> ReadReductionFromDb(DateTime startTime, DateTime endTime)
        {
            CeRepositoryManagerSfProxy ceRepoProxy = new CeRepositoryManagerSfProxy();
            List<Tuple<double, DateTime>> retVal = new List<Tuple<double, DateTime>>();

            try
            {
                var list = ceRepoProxy.GetTotalProductionsForSelectedTime(startTime, endTime);
                foreach (var item in list)
                {
                    retVal.Add(new Tuple<double, DateTime>((double)item.CO2Reduction, item.TimeOfCalculation.ToLocalTime()));
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


        public List<Tuple<double, DateTime>> ReadEmissionnFromDb(DateTime startTime, DateTime endTime)
        {
            CeRepositoryManagerSfProxy ceRepoProxy = new CeRepositoryManagerSfProxy();

            List<Tuple<double, DateTime>> retVal = new List<Tuple<double, DateTime>>();

            try
            {
                var list = ceRepoProxy.GetTotalProductionsForSelectedTime(startTime, endTime);
                foreach (var item in list)
                {
                    retVal.Add(new Tuple<double, DateTime>((double)item.CO2Emission, item.TimeOfCalculation.ToLocalTime()));
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
        public List<Tuple<double, DateTime>> ReadCostFromDb(DateTime startTime, DateTime endTime)
        {
            List<Tuple<double, DateTime>> retVal = new List<Tuple<double, DateTime>>();
            CeRepositoryManagerSfProxy ceRepoProxy = new CeRepositoryManagerSfProxy();

            try
            {
                var list = ceRepoProxy.GetTotalProductionsForSelectedTime(startTime, endTime);
                foreach (var item in list)
                {
                    retVal.Add(new Tuple<double, DateTime>((double)item.TotalCost, item.TimeOfCalculation.ToLocalTime()));
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

        //public List<Tuple<double, DateTime>> ReadProfitFromDb(DateTime startTime, DateTime endTime)
        //{
        //    List<Tuple<double, DateTime>> retVal = new List<Tuple<double, DateTime>>();
        //    CeRepositoryManagerSfProxy ceRepoProxy = new CeRepositoryManagerSfProxy("CeRepositoryManagerEndpoint");

        //    try
        //    {
        //        var list = ceRepoProxy.GetTotalProductionsForSelectedTime(startTime, endTime);
        //        foreach (var item in list)
        //        {
        //            retVal.Add(new Tuple<double, DateTime>((double)item.Profit, item.TimeOfCalculation.ToLocalTime()));
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        string message = string.Format("Failed read Measurements from database. {0}", e.Message);
        //        CommonTrace.WriteTrace(CommonTrace.TraceError, message);
        //        Console.WriteLine(message);
        //    }


        //    return retVal;
        //}


        #endregion

        #region Publish to UI

        private void PublishGeneratorsToUI(List<MeasurementUnit> measurementsFromGenerators)
		{
            CeRepositoryManagerSfProxy ceRepoProxy = new CeRepositoryManagerSfProxy();

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
            CePublishProxy.OptimizationResults(measListUI);
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
            CePublishProxy.OptimizationResults(measUIList);
        }

        private void PublishWindPercent(float windPercent)
        {
            CePublishProxy.WindPercentResult(windPercent);
        }

        private void PublishRenewableToUI( Tuple<DateTime, float> tupla)
        {
            CePublishProxy.RenewableResult(tupla);
        }


        private void PublishCoReductionToUI(Tuple<string, float, float> tupla)
        {
            CePublishProxy.PublishCoReduction(tupla);
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
                NetworkModelGDASfProxy networkModelGDASfProxy = new NetworkModelGDASfProxy();

                try
                {

                    // get all generators from NMS
                    properties = modelResourcesDesc.GetAllPropertyIds(ModelCode.GENERATOR);

                    iteratorId = networkModelGDASfProxy.GetExtentValues(ModelCode.GENERATOR, properties);
                    resourcesLeft = networkModelGDASfProxy.IteratorResourcesLeft(iteratorId);

                    while (resourcesLeft > 0)
                    {
                        List<ResourceDescription> rds = networkModelGDASfProxy.IteratorNext(numberOfResources, iteratorId);
                        retList.AddRange(rds);
                        resourcesLeft = networkModelGDASfProxy.IteratorResourcesLeft(iteratorId);
                    }
                    networkModelGDASfProxy.IteratorClose(iteratorId);

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
                    //NetworkModelGDAProxy.Instance = null;
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

                    iteratorId = networkModelGDASfProxy.GetExtentValues(ModelCode.ENERGY_CONSUMER, properties);
                    resourcesLeft = networkModelGDASfProxy.IteratorResourcesLeft(iteratorId);

                    while (resourcesLeft > 0)
                    {
                        List<ResourceDescription> rds = networkModelGDASfProxy.IteratorNext(numberOfResources, iteratorId);
                        retList.AddRange(rds);
                        resourcesLeft = networkModelGDASfProxy.IteratorResourcesLeft(iteratorId);
                    }
                    networkModelGDASfProxy.IteratorClose(iteratorId);

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
				FillInitialCommandedGenerators();

                FillInitialDiscreteCounters();
                
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

		private void FillInitialCommandedGenerators()
		{
            CeRepositoryManagerSfProxy ceRepoProxy = new CeRepositoryManagerSfProxy();

            List<CommandedGeneratorHelper> commandedGenerators = new List<CommandedGeneratorHelper>();
			commandedGenerators = ceRepoProxy.GetCommandedGenerators();


            List<CommandedGenerator> commandedGenerators1 = generators.Where(x => (x.Value.GeneratorType == GeneratorType.Coal ||
               x.Value.GeneratorType == GeneratorType.Gas || x.Value.GeneratorType == GeneratorType.Oil) && !commandedGenerators.Any(y => y.Gid == x.Value.GlobalId)).Select(y => new CommandedGenerator()
               {
                   Gid = y.Value.GlobalId,
                   CommandingFlag = false,
                   CommandingValue = 0,
                   PartitionKey = "CommandedGenerator",
                   RowKey = y.Value.GlobalId.ToString() + "_" + DateTime.Now.ToString("o")
                }).ToList();

            var listComGen = commandedGenerators1.Select(x => new CommandedGeneratorHelper(x)).ToList();

            ceRepoProxy.AddListCommandedGenerators(listComGen);
			
		}

        private void FillInitialDiscreteCounters()
        {
            string path = "C:/Users/barba/Desktop/EMS/ModelLabs/";

            XmlDocument doc = new XmlDocument();
            doc.Load(path + "ScadaProcessingSevice/MaxValDiscret.xml");

            XmlNodeList GeneratorStorageNode = doc.GetElementsByTagName("Generator");
            Dictionary<string, int> keyValues = new Dictionary<string, int>();
            foreach (XmlNode item in GeneratorStorageNode)
            {
                string type = item["GeneratorType"].InnerText;
                int maxTurnOn = int.Parse(item["MaxVal"].InnerText);
                keyValues.Add(type, maxTurnOn);
            }

            foreach (var item in generators)
            {
                MaxDiscreteCounter.Add(new Tuple<long, string>(item.Key, item.Value.GeneratorType.ToString()), keyValues[item.Value.GeneratorType.ToString()]);
            }
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

		public float CalculateCO2WithKyotoProtocol(Dictionary<long, OptimisationModel> optModelMap)
		{
			float kyotoCoefficient = 0.008f; //	EU - 0.8%
			return optModelMap.Values.Sum(x => (x.GenericOptimizedValue/1000f) * kyotoCoefficient);
		}

        public static float CalculateCO2ReductionWithBiggestCoeficient(Dictionary<long, OptimisationModel> optModelMap)
        {
            //CO2 Emissions from each fuel (tonnes) = Energy consumption of fuel (kWh) x Emission factor for each fuel (kgCO2/kWh) x 0.001
            float reductionCO2 = optModelMap.Values.Sum(x => (x.GenericOptimizedValue)) *0.30f * 0.001f;

            return reductionCO2;
        }

        public float GetProfit(Dictionary<long, OptimisationModel> allGenerators)
		{
			float profitValue = 0;
			Dictionary<GeneratorType, float> maxPowerPerFuel = new Dictionary<GeneratorType, float>();
			
			float sumOfRenewables = allGenerators.Where(x => x.Value.Renewable).Select(u => u.Value.measurementUnit.CurrentValue).Sum();

			maxPowerPerFuel.Add(GeneratorType.Oil, generators.Where(x => x.Value.GeneratorType == GeneratorType.Oil).Sum(y => y.Value.MaxQ));
			maxPowerPerFuel.Add(GeneratorType.Coal, generators.Where(x => x.Value.GeneratorType == GeneratorType.Coal).Sum(y => y.Value.MaxQ));
			maxPowerPerFuel.Add(GeneratorType.Gas, generators.Where(x => x.Value.GeneratorType == GeneratorType.Gas).Sum(y => y.Value.MaxQ));

			maxPowerPerFuel[GeneratorType.Oil] -= allGenerators.Where(x => x.Value.TypeGenerator == GeneratorType.Oil).Sum(y => y.Value.MeasuredValue);
			maxPowerPerFuel[GeneratorType.Coal] -= allGenerators.Where(x => x.Value.TypeGenerator == GeneratorType.Coal).Sum(y => y.Value.MeasuredValue);
			maxPowerPerFuel[GeneratorType.Gas] -= allGenerators.Where(x => x.Value.TypeGenerator == GeneratorType.Gas).Sum(y => y.Value.MeasuredValue);
			
			foreach (var item in allTypes)
			{
				if(maxPowerPerFuel[item.Key] >= sumOfRenewables)
				{
					//profitValue += (item.Value * (sumOfRenewables/1000f));
					float percentage = (100 * (sumOfRenewables / 1000f)) / (maxPowerPerFuel[item.Key]/1000f);
					GeneratorCurveModel genCurveModel = generatorCurves.FirstOrDefault(x => x.LowerPoint <= percentage && x.HigherPoint >= percentage && x.GeneratorType.Contains(item.Key.ToString()));
					float fuelQuantityPerMW = (float)genCurveModel.A * percentage + (float)genCurveModel.B;       //[t/MW]
					float fuelQuantity = fuelQuantityPerMW * sumOfRenewables / 1000f;
					profitValue += item.Value * fuelQuantity;
					break;
				}
				else
				{
					sumOfRenewables -= maxPowerPerFuel[item.Key];
					float percentage = (100 * (sumOfRenewables / 1000f)) / (maxPowerPerFuel[item.Key] / 1000f);
					percentage = percentage / 100f;
					GeneratorCurveModel genCurveModel = generatorCurves.FirstOrDefault(x => x.LowerPoint <= percentage && x.HigherPoint >= percentage && x.GeneratorType.Contains(item.Key.ToString()));
					float fuelQuantityPerMW = (float)genCurveModel.A * percentage + (float)genCurveModel.B;       //[t/MW]
					float fuelQuantity = fuelQuantityPerMW * sumOfRenewables / 1000f;
					profitValue += item.Value * fuelQuantity;
					//profitValue += (item.Value * (sumOfRenewables / 1000f));
				}
			}
			return profitValue;
		}

		private float GetMaxPowerForCurrentGenerator(GeneratorType generatorType)
		{
			return generators.Where(x => x.Value.GeneratorType == generatorType).Select(x => x.Value.MaxQ).Sum();
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

		public static bool SetPricePerGeneratorType(float oilPrice, float coalPrice, float gasPrice)
		{
			allTypes[GeneratorType.Coal] = coalPrice < 0 ? 0 : coalPrice;
			allTypes[GeneratorType.Gas] = gasPrice < 0 ? 0 : gasPrice;
			allTypes[GeneratorType.Oil] = oilPrice < 0 ? 0 : oilPrice;

			var myList = allTypes.ToList();
			myList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
			allTypes = myList.ToDictionary(pair => pair.Key, pair => pair.Value);
			return true;
		}

		public static bool SetPricePerGeneratorTypeDefault()
		{
			allTypes = new Dictionary<GeneratorType, float>();

			allTypes[GeneratorType.Coal] = 1;
			allTypes[GeneratorType.Gas] = 2;
			allTypes[GeneratorType.Oil] = 3;
			return true;
		}

		public static Tuple<int, int, int, float> GetAlgorithmParams()
        {
            return new Tuple<int, int, int, float>(NUMBER_OF_ITERATION, NUMBER_OF_POPULATION, ELITIMS_PERCENTAGE, mutationRate);
        }

		public static Tuple<float, float, float> GetPricePerGeneratorTypes()
		{
			return new Tuple<float, float, float>(allTypes[GeneratorType.Oil], allTypes[GeneratorType.Coal], allTypes[GeneratorType.Gas]);
		}

		private Dictionary<GeneratorType,float> FillPricePerGeneratorType()
		{
			var ret = new Dictionary<GeneratorType, float>();

			ret.Add(GeneratorType.Coal, 1f);
			ret.Add(GeneratorType.Gas, 2f);
			ret.Add(GeneratorType.Oil, 3f);
			return ret;
		}

		public void ResetCommandedGenerator(long gid)
		{
            CeRepositoryManagerSfProxy ceRepoProxy = new CeRepositoryManagerSfProxy();

            var commandedGen = ceRepoProxy.GetCommandedGenerator(gid);
			if(commandedGen != null)
			{
				commandedGen.CommandingFlag = false;
				commandedGen.CommandingValue = 0;
                ceRepoProxy.AddCommandedGenerator(commandedGen);
			}	
		}

		public List<float> GetPointForFuelEconomy(long gid)
		{
			List<float> points = new List<float>();
			points.Add(optimizationModelResults[gid].PointX);
			points.Add(optimizationModelResults[gid].PointY);
			return points;
		}
	}
}
