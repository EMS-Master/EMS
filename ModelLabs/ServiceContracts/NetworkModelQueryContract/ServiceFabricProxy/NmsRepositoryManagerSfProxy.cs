using CommonCloud;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTN.ServiceContracts.ServiceFabricProxy
{
    public class NmsRepositoryManagerSfProxy : INmsRepositoryManager
    {
        private ServicePartitionResolver resolver = ServicePartitionResolver.GetDefault();

        private WcfCommunicationClientFactory<INmsRepositoryManager> factory;
        private ServicePartitionClient<WcfCommunicationClient<INmsRepositoryManager>> proxy;

        public NmsRepositoryManagerSfProxy()
        {
            factory = new WcfCommunicationClientFactory<INmsRepositoryManager>(
                   clientBinding: Binding.CreateCustomNetTcp(),
                   servicePartitionResolver: resolver);

            proxy = new ServicePartitionClient<WcfCommunicationClient<INmsRepositoryManager>>(
                    communicationClientFactory: factory,
                    serviceUri: new Uri("fabric:/CloudEMS/NmsRepositoryManagerService"),
                    listenerName: "NmsRepositoryManagerEndpoint");
        }

        public void AddDelta(byte[] delta)
        {
            try
            {
                proxy.InvokeWithRetry(x => x.Channel.AddDelta(delta));
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public byte[] ReadDelta()
        {
            try
            {
                return proxy.InvokeWithRetry(x => x.Channel.ReadDelta());
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
