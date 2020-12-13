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
using TransactionContract;

namespace TransactionManagerService.ServiceFabricProxy
{
    public class TransactionNMSSfProxy : ITransactionContract
    {
        private ServicePartitionResolver resolver = ServicePartitionResolver.GetDefault();

        private WcfCommunicationClientFactory<ITransactionContract> factory;
        private ServicePartitionClient<WcfCommunicationClient<ITransactionContract>> proxy;

        public TransactionNMSSfProxy()
        {
            factory = new WcfCommunicationClientFactory<ITransactionContract>(
                    clientBinding: Binding.CreateCustomNetTcp(),
                    servicePartitionResolver: resolver,
                    callback: new TransactionManager());

            proxy = new ServicePartitionClient<WcfCommunicationClient<ITransactionContract>>(
                    communicationClientFactory: factory,
                    serviceUri: new Uri("fabric:/EMSServiceFab/NetworkModelCloudService"),
                    listenerName: "NMSTranscationEndpoint");
        }

        public bool Commit()
        {
            return proxy.InvokeWithRetry(x => x.Channel.Commit());
        }

        public UpdateResult Prepare(ref Delta delta)
        {
            UpdateResult result = new UpdateResult();
            Delta temp = delta;
            result = proxy.InvokeWithRetry(x => x.Channel.Prepare(ref temp)); //PROVERITI ZA SVAKI SLUCAJ
            delta = temp;
            return result;
        }

        public bool Rollback()
        {
            return proxy.InvokeWithRetry(x => x.Channel.Rollback());
        }
    }
}
