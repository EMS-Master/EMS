using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonCloud;
using FTN.Common;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using TransactionContract;

namespace TransactionManagerService.ServiceFabricProxy
{
    public class TransactionScadaPRSfProxy : ITransactionContract
    {
        private ServicePartitionResolver resolver = ServicePartitionResolver.GetDefault();

        private WcfCommunicationClientFactory<ITransactionContract> factory;
        private ServicePartitionClient<WcfCommunicationClient<ITransactionContract>> proxy;

        public TransactionScadaPRSfProxy()
        {
            factory = new WcfCommunicationClientFactory<ITransactionContract>(
                    clientBinding: Binding.CreateCustomNetTcp(),
                    servicePartitionResolver: resolver,
                    callback: new TransactionManager());

            proxy = new ServicePartitionClient<WcfCommunicationClient<ITransactionContract>>(
                    communicationClientFactory: factory,
                    serviceUri: new Uri("fabric:/EMS/ScadaProcessingCloudServ"),
                    listenerName: "TransactionPREndpoint");
        }
        public bool Commit()
        {
            return proxy.InvokeWithRetry(x => x.Channel.Commit());
        }

        public UpdateResult Prepare(ref Delta delta)
        {
            Delta temp = delta;
            return proxy.InvokeWithRetry(x => x.Channel.Prepare(ref temp)); //PROVJERITI ZA SVAKI SLUCAJ
        }

        public bool Rollback()
        {
            return proxy.InvokeWithRetry(x => x.Channel.Rollback());
        }
    }
}
