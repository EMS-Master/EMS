using CloudCommon;
using CommonMeas;
using FTN.ServiceContracts;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlarmsEventsContract.ServiceFabricProxy
{
    public class AesIntegritySfProxy : IAesIntegirtyContract
    {
        private ServicePartitionResolver resolver = ServicePartitionResolver.GetDefault();

        private WcfCommunicationClientFactory<IAesIntegirtyContract> factory;
        private ServicePartitionClient<WcfCommunicationClient<IAesIntegirtyContract>> proxy;

        public AesIntegritySfProxy()
        {
            factory = new WcfCommunicationClientFactory<IAesIntegirtyContract>(
                    clientBinding: Binding.CreateCustomNetTcp(),
                    servicePartitionResolver: resolver);

            proxy = new ServicePartitionClient<WcfCommunicationClient<IAesIntegirtyContract>>(
                    communicationClientFactory: factory,
                    serviceUri: new Uri("fabric:/EMSServiceFab/AlarmsEventsCloudService"),
                    listenerName: "AlarmsEventsIntegrityEndpoint",
                    partitionKey: ServicePartitionKey.Singleton);
        }

        public List<AlarmHelper> InitiateIntegrityUpdate()
        {
            return proxy.InvokeWithRetry(x => x.Channel.InitiateIntegrityUpdate());
        }
    }
}
