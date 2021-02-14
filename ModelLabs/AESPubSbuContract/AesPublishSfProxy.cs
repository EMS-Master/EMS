using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonCloud;
using CommonMeas;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;

namespace AESPubSbuContract
{
    public class AesPublishSfProxy : IAesPublishContract
    {
        private ServicePartitionResolver resolver = ServicePartitionResolver.GetDefault();

        private WcfCommunicationClientFactory<IAesPublishContract> factory;
        private ServicePartitionClient<WcfCommunicationClient<IAesPublishContract>> proxy;

        public AesPublishSfProxy()
        {
            factory = new WcfCommunicationClientFactory<IAesPublishContract>(
                    clientBinding: Binding.CreateCustomNetTcp(),
                    servicePartitionResolver: resolver);

            proxy = new ServicePartitionClient<WcfCommunicationClient<IAesPublishContract>>(
                    communicationClientFactory: factory,
                    serviceUri: new Uri("fabric:/CloudEMS/AlarmsEventsCloudServ"),
                    listenerName: "AESPublishEndpoint");
        }

        public void PublishAlarmsEvents(AlarmHelper alarm, PublishingStatus status)
        {
            proxy.InvokeWithRetry(x => x.Channel.PublishAlarmsEvents(alarm, status));
        }

        public void PublishStateChange(AlarmHelper alarm)
        {
            proxy.InvokeWithRetry(x => x.Channel.PublishStateChange(alarm));
        }
    }
}
