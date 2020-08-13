using CalculationEngineServ.DataBaseModels;
using CalculationEngineServ.GeneticAlgorithm;
using CalculationEngineServ.PubSub;
using CommonMeas;
using FTN.Common;
using FTN.ServiceContracts;
using ScadaContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using TransactionContract;

namespace CalculationEngineServ
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	public class CalculationEngine : ITransactionContract
    {
		private PublisherService publisher = null;

        private float totalProduction = 0;

        public CalculationEngine()
		{
			publisher = new PublisherService();
		}
		public bool Commit()
        {
            throw new NotImplementedException();
        }

        public bool Optimize(List<MeasurementUnit> measEnergyConsumer, List<MeasurementUnit> measGenerators, float windData, float sunData)
        {
            bool result = false;
            totalProduction = 0;

            foreach (var m in measGenerators)
            {
                Console.WriteLine("masx value: " + m.MaxValue);
            }
            GA ga = new GA();

            //todo optimization and put into measuremnetOptimized list
            //for now, lets put parameter measGenerators
            List<MeasurementUnit> measurementsOptimized = measGenerators;
            if (InsertMeasurementsIntoDb(measurementsOptimized))
            {
                Console.WriteLine("Inserted {0} Measurement(s) into history database.", measurementsOptimized.Count);
            }

			PublishGeneratorsToUI(measurementsOptimized);

			try
            {
                if (measurementsOptimized != null && measurementsOptimized.Count > 0)
                {
                    totalProduction = measurementsOptimized.Sum(x => x.CurrentValue);

                    if (WriteTotalProductionIntoDb(totalProduction, DateTime.Now))
                    {
                        Console.WriteLine("The total production is recorded into history database.");
                    }
                }
                if (ScadaCommandingProxy.Instance.SendDataToSimulator(measurementsOptimized))
                {
                    CommonTrace.WriteTrace(CommonTrace.TraceInfo, "CE sent {0} optimized MeasurementUnit(s) to SCADACommanding.", measurementsOptimized.Count);
                    Console.WriteLine("CE sent {0} optimized MeasurementUnit(s) to SCADACommanding.", measurementsOptimized.Count);

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


    }
}
