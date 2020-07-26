using CalculationEngineServ.DataBaseModels;
using CalculationEngineServ.GeneticAlgorithm;
using CommonMeas;
using FTN.Common;
using ScadaContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionContract;

namespace CalculationEngineServ
{
    public class CalculationEngine : ITransactionContract
    {
        public bool Commit()
        {
            throw new NotImplementedException();
        }

        public bool Optimize(List<MeasurementUnit> measBatteryStorage, List<MeasurementUnit> measGenerators, float windData, float sunData)
        {
            bool result = false;
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

            try
            {
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
    }
}
