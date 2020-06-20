using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ScadaContracts
{
    public class ScadaProcessingProxy : IScadaProcessingContract, IDisposable
    {
        private static IScadaProcessingContract proxy;
        private static ChannelFactory<IScadaProcessingContract> factory;
        private static object lockObj = new object();

        
        public static IScadaProcessingContract Instance
        {
            get
            {
                lock (lockObj)
                {
                    if (proxy == null)
                    {
                        factory = new ChannelFactory<IScadaProcessingContract>("*");
                        proxy = factory.CreateChannel();
                    }

                    return proxy;
                }
            }

            set
            {
                lock (lockObj)
                {
                    if (proxy == null)
                    {
                        proxy = value;
                    }
                }
            }
        }

        public void Dispose()
        {
            if (factory != null)
            {
                factory = null;
            }
        }

        public bool SendValues(byte[] value, bool[] valueDiscrete)
        {
            lock (lockObj)
            {
                return proxy.SendValues(value, valueDiscrete);
            }
        }
    }
}
