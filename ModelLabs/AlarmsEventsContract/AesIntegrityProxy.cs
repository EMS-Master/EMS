using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using CalculationEngineServ.DataBaseModels;
using CommonMeas;

namespace FTN.ServiceContracts
{
    public class AesIntegrityProxy : IAesIntegirtyContract, IDisposable
    {
        private static IAesIntegirtyContract proxy = null;
        private static ChannelFactory<IAesIntegirtyContract> factory = null;

        public static IAesIntegirtyContract Instance
        {
            get
            {
                if (proxy == null)
                {
                    factory = new ChannelFactory<IAesIntegirtyContract>("AesIntegrityEndpoint");
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
                Console.WriteLine("Communication exception: {0}", ce.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("AE integrity proxy exception: {0}", e.Message);
            }
        }

        public List<Alarm> InitiateIntegrityUpdate()
        {
            return proxy.InitiateIntegrityUpdate();
        }
    }
}
