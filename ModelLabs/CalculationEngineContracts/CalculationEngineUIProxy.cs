using CommonCloud.AzureStorage.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineContracts
{
    public class CalculationEngineUIProxy : ICalculationEngineUIContract, IDisposable
    {
        private static ICalculationEngineUIContract proxy;
        private static ChannelFactory<ICalculationEngineUIContract> factory;

        public static ICalculationEngineUIContract Instance
        {
            get
            {
                if (proxy == null)
                {
                    factory = new ChannelFactory<ICalculationEngineUIContract>("*");
                    proxy = factory.CreateChannel();
                    IContextChannel cc = proxy as IContextChannel;
                }

                return proxy;
            }

            set
            {
                if (proxy == null)
                {
                    proxy = value;
                }
            }
        }

        public void Dispose()
        {
            try
            {
                if (factory != null)
                {
                    factory = null;
                }
            }
            catch (CommunicationException ce)
            {
                Console.WriteLine("Communication exception: {0}", ce.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("CE proxy exception: {0}", e.Message);
            }
        }

        public Tuple<int, int, int, float> GetAlgorithmOptions()
        {
            return proxy.GetAlgorithmOptions();
        }

        public List<Tuple<double, DateTime>> GetCoEmission(DateTime startTime, DateTime endTime)
        {
            return proxy.GetCoEmission(startTime, endTime);
        }

        public List<Tuple<double, DateTime>> GetCoReduction(DateTime startTime, DateTime endTime)
        {
            return proxy.GetCoReduction(startTime, endTime);
        }

        public List<Tuple<double, DateTime>> GetCost(DateTime startTime, DateTime endTime)
        {
            return proxy.GetCost(startTime, endTime);
        }

        public List<Tuple<double, DateTime>> GetHistoryMeasurements(long gid, DateTime startTime, DateTime endTime)
        {
            return proxy.GetHistoryMeasurements(gid, startTime, endTime);
        }

		public Tuple<float, float, float> GetPricePerGeneratorType()
		{
			return proxy.GetPricePerGeneratorType(); 
		}

        public List<Tuple<double, DateTime>> GetProfit(DateTime startTime, DateTime endTime)
        {
            return proxy.GetProfit(startTime, endTime);

        }

        public List<Tuple<DateTime, double, double, double, double, double>> GetTotalProduction(DateTime startTime, DateTime endTime)
        {
            return proxy.GetTotalProduction(startTime, endTime);
        }

        public bool SetAlgorithmOptions(int iterationCount, int populationCount, int elitisamPct, float mutationRate)
        {
            return proxy.SetAlgorithmOptions(iterationCount, populationCount,  elitisamPct, mutationRate);
        }
        
        public bool SetAlgorithmOptionsDefault()
        {
            return proxy.SetAlgorithmOptionsDefault();
        }

		public bool SetPricePerGeneratorType(float oilPrice, float coalPrice, float gasPrice)
		{
			return proxy.SetPricePerGeneratorType(oilPrice, coalPrice, gasPrice);
		}

		public bool SetPricePerGeneratorTypeDefault()
		{
			return proxy.SetPricePerGeneratorTypeDefault();
		}

		public void ResetCommandedGenerator(long gid)
		{
			proxy.ResetCommandedGenerator(gid);
		}

		public List<float> GetPointForFuelEconomy(long gid)
		{
			return proxy.GetPointForFuelEconomy(gid);
		}
	}
}
