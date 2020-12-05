using CloudCommon;
using CommonMeas;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScadaContracts.ServiceFabricProxy
{
    public class ScadaCMDsfProxy : IScadaCommandingContract
    {
        private ServicePartitionResolver resolver = ServicePartitionResolver.GetDefault();

        private WcfCommunicationClientFactory<IScadaCommandingContract> factory;
        private ServicePartitionClient<WcfCommunicationClient<IScadaCommandingContract>> proxy;

        public ScadaCMDsfProxy()
        {
            factory = new WcfCommunicationClientFactory<IScadaCommandingContract>(
                    clientBinding: Binding.CreateCustomNetTcp(),
                    servicePartitionResolver: resolver);
            //callback

            proxy = new ServicePartitionClient<WcfCommunicationClient<IScadaCommandingContract>>(
                    communicationClientFactory: factory,
                    serviceUri: new Uri("fabric:/EMSServiceFab/ScadaCommandingCloudService"),
                    listenerName: "ScadaCMDEndpoint");
        }

        public bool CommandAnalogValues(long gid, float value)
        {
            return proxy.InvokeWithRetry(x => x.Channel.CommandAnalogValues(gid, value));
        }

        public bool CommandDiscreteValues(long gid, bool value)
        {
            return proxy.InvokeWithRetry(x => x.Channel.CommandDiscreteValues(gid, value));
        }

        public bool SendDataToSimulator(List<MeasurementUnit> measurements)
        {
            return proxy.InvokeWithRetry(x => x.Channel.SendDataToSimulator(measurements));
        }
    }
}
