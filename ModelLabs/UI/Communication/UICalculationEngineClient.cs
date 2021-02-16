using CalculationEngineContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace UI.Communication
{
    public class UICalculationEngineClient : ClientBase<ICalculationEngineUIContract>, ICalculationEngineUIContract
    {
        public UICalculationEngineClient() { }
        public UICalculationEngineClient(string Endpoint) : base(Endpoint) { }
        public Tuple<int, int, int, float> GetAlgorithmOptions()
        {
            return Channel.GetAlgorithmOptions();
        }

        public List<Tuple<double, DateTime>> GetCoEmission(DateTime startTime, DateTime endTime)
        {
            return Channel.GetCoEmission(startTime, endTime);
        }

        public List<Tuple<double, DateTime>> GetCoReduction(DateTime startTime, DateTime endTime)
        {
            return Channel.GetCoReduction(startTime, endTime);
        }

        public List<Tuple<double, DateTime>> GetCost(DateTime startTime, DateTime endTime)
        {
            return Channel.GetCost(startTime, endTime);
        }

        public List<Tuple<double, DateTime>> GetHistoryMeasurements(long gid, DateTime startTime, DateTime endTime)
        {
            return Channel.GetHistoryMeasurements(gid, startTime, endTime);
        }

        public List<float> GetPointForFuelEconomy(long gid)
        {
            return Channel.GetPointForFuelEconomy(gid);
        }

        public Tuple<float, float, float> GetPricePerGeneratorType()
        {
            return Channel.GetPricePerGeneratorType();
        }

        public List<Tuple<double, DateTime>> GetProfit(DateTime startTime, DateTime endTime)
        {
            return Channel.GetProfit(startTime, endTime);
        }

        public List<Tuple<double, DateTime>> GetTotalProduction(DateTime startTime, DateTime endTime)
        {
            return Channel.GetTotalProduction(startTime, endTime);
        }

        public void ResetCommandedGenerator(long gid)
        {
             Channel.ResetCommandedGenerator(gid);
        }

        public bool SetAlgorithmOptions(int iterationCount, int populationCount, int elitisamPct, float mutationRate)
        {
            return Channel.SetAlgorithmOptions(iterationCount, populationCount, elitisamPct, mutationRate);
        }

        public bool SetAlgorithmOptionsDefault()
        {
            return Channel.SetAlgorithmOptionsDefault();
        }

        public bool SetPricePerGeneratorType(float oilPrice, float coalPrice, float gasPrice)
        {
            return Channel.SetPricePerGeneratorType(oilPrice, coalPrice, gasPrice);
        }

        public bool SetPricePerGeneratorTypeDefault()
        {
            return Channel.SetPricePerGeneratorTypeDefault();
        }
    }
}
