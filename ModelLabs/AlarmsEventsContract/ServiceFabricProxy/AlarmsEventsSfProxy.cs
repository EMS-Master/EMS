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
    public class AlarmsEventsSfProxy : IAlarmsEventsContract
    {
        private ServicePartitionResolver resolver = ServicePartitionResolver.GetDefault();

        private WcfCommunicationClientFactory<IAlarmsEventsContract> factory;
        private ServicePartitionClient<WcfCommunicationClient<IAlarmsEventsContract>> proxy;

        public AlarmsEventsSfProxy()
        {
            factory = new WcfCommunicationClientFactory<IAlarmsEventsContract>(
                    clientBinding: CommonCloud.Binding.CreateCustomNetTcp(),
                    servicePartitionResolver: resolver);

            proxy = new ServicePartitionClient<WcfCommunicationClient<IAlarmsEventsContract>>(
                    communicationClientFactory: factory,
                    serviceUri: new Uri("fabric:/CloudEMS/AlarmsEventsCloudServ"),
                    listenerName: "AlarmsEventsEndpoint");
        }

        public void AddAlarm(AlarmHelper alarm)
        {
            proxy.InvokeWithRetry(x => x.Channel.AddAlarm(alarm));
        }

        public void UpdateAckStatus(AlarmHelper alarmtoupdate)
        {
            proxy.InvokeWithRetry(x => x.Channel.UpdateAckStatus(alarmtoupdate));
        }

        public void UpdateStatus(AnalogLocation analogLoc, State state)
        {
            proxy.InvokeWithRetry(x => x.Channel.UpdateStatus(analogLoc, state));
        }
    }
}
