using CalculationEngineContracts;
using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineCloudServ
{
    public class CeToUICloud : ICalculationEngineUIContract
    {
        private static CalculationEngineCloud ce = null;
        public CeToUICloud()
        {
        }

        public static CalculationEngineCloud Ce { set => ce = value; }

        public Tuple<int, int, int, float> GetAlgorithmOptions()
        {
            return ce.GetAlgorithmParams();
        }

        public List<Tuple<double, DateTime>> GetHistoryMeasurements(long gid, DateTime startTime, DateTime endTime)
        {
            List<Tuple<double, DateTime>> retList = new List<Tuple<double, DateTime>>();
            try
            {
                retList = ce.ReadMeasurementsFromDb(gid, startTime, endTime);
            }
            catch (Exception ex)
            {
                CommonTrace.WriteTrace(CommonTrace.TraceError, "[CeToUI] Error GetHistoryMeasurements {0}", ex.Message);
            }

            return retList;
        }

        //public List<Tuple<double, DateTime>> GetTotalProduction(DateTime startTime, DateTime endTime)
        //{
        //	List<Tuple<double, DateTime>> retList = new List<Tuple<double, DateTime>>();

        //	try
        //	{
        //		retList = ce.ReadTotalProductionsFromDb(startTime, endTime);
        //	}
        //	catch (Exception ex)
        //	{
        //		CommonTrace.WriteTrace(CommonTrace.TraceError, "[CeToUI] Error GetTotalProduction {0}", ex.Message);
        //	}

        //	return retList;
        //}

        public List<Tuple<DateTime, double, double, double, double, double>> GetTotalProduction(DateTime startTime, DateTime endTime)
        {
            try
            {
                return ce.ReadTotalProductions(startTime, endTime);
            }
            catch (Exception ex)
            {
                CommonTrace.WriteTrace(CommonTrace.TraceError, "[CeToUI] Error GetTotalProduction {0}", ex.Message);
            }

            return new List<Tuple<DateTime, double, double, double, double, double>>();
        }


        public bool SetAlgorithmOptions(int iterationCount, int populationCount, int elitisamPct, float mutationRate)
        {
            return ce.SetAlgorithmParams(iterationCount, populationCount, elitisamPct, mutationRate);
        }
        public bool SetAlgorithmOptionsDefault()
        {
            return ce.SetAlgorithmParamsDefault();
        }

        public bool SetPricePerGeneratorType(float oilPrice, float coalPrice, float gasPrice)
        {
            return ce.SetPricePerGeneratorType(oilPrice, coalPrice, gasPrice);
        }

        public bool SetPricePerGeneratorTypeDefault()
        {
            return ce.SetPricePerGeneratorTypeDefault();
        }

        public Tuple<float, float, float> GetPricePerGeneratorType()
        {
            return ce.GetPricePerGeneratorTypes();
        }

        public List<Tuple<double, DateTime>> GetProfit(DateTime startTime, DateTime endTime)
        {
            List<Tuple<double, DateTime>> retList = new List<Tuple<double, DateTime>>();

            try
            {
                retList = ce.ReadTotalProfitFromDb(startTime, endTime);
            }
            catch (Exception ex)
            {
                CommonTrace.WriteTrace(CommonTrace.TraceError, "[CeToUI] Error GetTotalProduction {0}", ex.Message);
            }

            return retList;
        }

        public List<Tuple<double, DateTime>> GetCoReduction(DateTime startTime, DateTime endTime)
        {
            List<Tuple<double, DateTime>> retList = new List<Tuple<double, DateTime>>();

            try
            {
                retList = ce.ReadReductionFromDb(startTime, endTime);
            }
            catch (Exception ex)
            {
                CommonTrace.WriteTrace(CommonTrace.TraceError, "[CeToUI] Error GetTotalProduction {0}", ex.Message);
            }

            return retList;
        }

        public List<Tuple<double, DateTime>> GetCoEmission(DateTime startTime, DateTime endTime)
        {
            List<Tuple<double, DateTime>> retList = new List<Tuple<double, DateTime>>();

            try
            {
                retList = ce.ReadEmissionnFromDb(startTime, endTime);
            }
            catch (Exception ex)
            {
                CommonTrace.WriteTrace(CommonTrace.TraceError, "[CeToUI] Error GetTotalProduction {0}", ex.Message);
            }

            return retList;
        }

        public List<Tuple<double, DateTime>> GetCost(DateTime startTime, DateTime endTime)
        {
            List<Tuple<double, DateTime>> retList = new List<Tuple<double, DateTime>>();

            try
            {
                retList = ce.ReadCostFromDb(startTime, endTime);
            }
            catch (Exception ex)
            {
                CommonTrace.WriteTrace(CommonTrace.TraceError, "[CeToUI] Error GetTotalProduction {0}", ex.Message);
            }

            return retList;
        }

        public void ResetCommandedGenerator(long gid)
        {
            try
            {
                ce.ResetCommandedGenerator(gid);
            }
            catch (Exception ex)
            {
                CommonTrace.WriteTrace(CommonTrace.TraceError, "[CeToUI] Error ResetCommandedGenerator {0}", ex.Message);
            }
        }

        public List<float> GetPointForFuelEconomy(long gid)
        {
            List<float> points = new List<float>();
            try
            {
                points = ce.GetPointForFuelEconomy(gid);
            }
            catch (Exception ex)
            {
                CommonTrace.WriteTrace(CommonTrace.TraceError, "[CeToUI] Error GetPointForFuelEconomy {0}", ex.Message);
            }
            return points;
        }
    }
}
