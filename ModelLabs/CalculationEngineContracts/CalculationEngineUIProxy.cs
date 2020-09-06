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

        public List<Tuple<double, DateTime>> GetHistoryMeasurements(long gid, DateTime startTime, DateTime endTime)
        {
            return proxy.GetHistoryMeasurements(gid, startTime, endTime);
        }

        public List<Tuple<double, DateTime>> GetTotalProduction(DateTime startTime, DateTime endTime)
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
    }
}
