using CloudCommon;
using FTN.Common;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionContract.ServiceFabricProxy
{
    public class ImporterSfProxy : IImporterContract
    {
        private ServicePartitionResolver resolver = ServicePartitionResolver.GetDefault();

        private WcfCommunicationClientFactory<IImporterContract> factory;
        private ServicePartitionClient<WcfCommunicationClient<IImporterContract>> proxy;

        public ImporterSfProxy()
        {
            factory = new WcfCommunicationClientFactory<IImporterContract>(
                    clientBinding: Binding.CreateCustomNetTcp(),
                    servicePartitionResolver: resolver);

            proxy = new ServicePartitionClient<WcfCommunicationClient<IImporterContract>>(
                    communicationClientFactory: factory,
                    serviceUri: new Uri("fabric:/EMS/TransactionManagerCloudService"),
                    listenerName: "TransactionManagerEndpoint");
        }
        public UpdateResult ModelUpdate(Delta delta)
        {
            return proxy.InvokeWithRetry(x => x.Channel.ModelUpdate(delta));
        }
    }
}
