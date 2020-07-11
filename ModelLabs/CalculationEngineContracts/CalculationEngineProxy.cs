using CommonMeas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineContracts
{
    public class CalculationEngineProxy : ICalculationEngineRepository, ICalculationEngineContract, IDisposable
    {
        /// <summary>
        /// proxy object
        /// </summary>
        private static ICalculationEngineContract proxy;
        private static ICalculationEngineRepository proxy1;

        /// <summary>
        /// ChannelFactory object
        /// </summary>
        private static ChannelFactory<ICalculationEngineContract> factory;
        private static ChannelFactory<ICalculationEngineRepository> factory1;

        /// <summary>
        /// Gets or sets instance of ICalculationEngineContract
        /// </summary>
        public static ICalculationEngineContract Instance
        {
            get
            {
                if (proxy == null)
                {
                    factory = new ChannelFactory<ICalculationEngineContract>("*");
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

        /// <summary>
        /// Gets or sets instance of ICalculationEngineContract
        /// </summary>
        public static ICalculationEngineRepository InstanceRepository
        {
            get
            {
                if (proxy1 == null)
                {
                    factory1 = new ChannelFactory<ICalculationEngineRepository>("*");
                    proxy1 = factory1.CreateChannel();
                    IContextChannel cc = proxy1 as IContextChannel;
                }

                return proxy1;
            }

            set
            {
                if (proxy1 == null)
                {
                    proxy1 = value;
                }
            }
        }

        /// <summary>
        /// Dispose method
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (factory != null)
                {
                    factory = null;
                }
                if (factory1 != null)
                {
                    factory1 = null;
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

        /// <summary>
        /// Optimization algorithm
        /// </summary>
        /// <param name="measEnergyConsumers">list of measurements for energyConsumers</param>
        /// <param name="measGenerators">list of measurements for generators</param>
        /// <param name="windSpeed">speed of wind</param>
        /// <param name="sunlight">sunlight percent</param>
        /// <returns>returns true if optimization was successful</returns>
        public bool OptimisationAlgorithm(List<MeasurementUnit> measBatteryStorage, List<MeasurementUnit> measGenerators)
        {
            return proxy.OptimisationAlgorithm(measBatteryStorage, measGenerators);
        }

        public List<DiscreteCounterModel> GetAllDiscreteCounters()
        {
            return proxy1.GetAllDiscreteCounters();
        }

        
        public void InsertOrUpdate(DiscreteCounterModel model)
        {
            proxy1.InsertOrUpdate(model);
        }
    }
}
