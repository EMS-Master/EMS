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
    public class ScadaProcessingSfProxy : IScadaProcessingContract
    {
        private ServicePartitionResolver resolver = ServicePartitionResolver.GetDefault();

        private WcfCommunicationClientFactory<IScadaProcessingContract> factory;
        private ServicePartitionClient<WcfCommunicationClient<IScadaProcessingContract>> proxy;

        public ScadaProcessingSfProxy()
        {
            factory = new WcfCommunicationClientFactory<IScadaProcessingContract>(
                    clientBinding: CommonCloud.Binding.CreateCustomNetTcp(),
                    servicePartitionResolver: resolver);
            //callback

            proxy = new ServicePartitionClient<WcfCommunicationClient<IScadaProcessingContract>>(
                    communicationClientFactory: factory,
                    serviceUri: new Uri("fabric:/CloudEMS/ScadaProcessingCloudServ"),
                    listenerName: "ScadaCREndpoint");
        }

        public bool SendValues(byte[] value, bool[] valueDiscrete, byte[] valueWindSun)
        {
            return proxy.InvokeWithRetry(x => x.Channel.SendValues(value,valueDiscrete, valueWindSun));
        }
    }
}
