using CommonMeas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ScadaContracts
{
    public class ScadaCommandingProxy : IScadaCommandingContract, IDisposable
    {
        private static IScadaCommandingContract proxy;
        private static ChannelFactory<IScadaCommandingContract> factory;

        public static IScadaCommandingContract Instance
        {
            get
            {
                if (proxy == null)
                {
                    factory = new ChannelFactory<IScadaCommandingContract>("*");
                    proxy = factory.CreateChannel();
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
                Console.WriteLine("ScadaCMDProxy Communication exception: {0}", ce.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("ScadaCMDProxy exception: {0}", e.Message);
            }
        }

        public bool SendDataToSimulator(List<MeasurementUnit> measurements)
        {
            return proxy.SendDataToSimulator(measurements);
        }

        public bool CommandDiscreteValues(long gid, bool value, int scadaAddress)
        {
            return proxy.CommandDiscreteValues(gid, value, scadaAddress);
        }
        public bool CommandAnalogValues(long gid, float value)
        {
            return proxy.CommandAnalogValues(gid, value);
        }
    }
}
