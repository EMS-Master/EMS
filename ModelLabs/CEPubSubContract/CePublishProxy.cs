using CloudCommon;
using EMS.ServiceContracts;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTN.ServiceContracts
{
    public class CePublishProxy : ICePublishContract
    {

        private ServicePartitionResolver resolver = ServicePartitionResolver.GetDefault();

        private WcfCommunicationClientFactory<ICePublishContract> factory;
        private ServicePartitionClient<WcfCommunicationClient<ICePublishContract>> proxy;
        public CePublishProxy()
        {
            factory = new WcfCommunicationClientFactory<ICePublishContract>(
                    clientBinding: Binding.CreateCustomNetTcp(),
                    servicePartitionResolver: resolver);

            proxy = new ServicePartitionClient<WcfCommunicationClient<ICePublishContract>>(
                    communicationClientFactory: factory,
                    serviceUri: new Uri("fabric:/EMSServiceFab/GatewayService"),
                    listenerName: "CEPublishEndpoint",
                    partitionKey: new ServicePartitionKey(0));
        }

        public void PublishOptimizationResults(List<MeasurementUI> result)
        {
            proxy.InvokeWithRetry(x => x.Channel.PublishOptimizationResults(result));
        }
    }
}
