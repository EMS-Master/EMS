using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FTN.Common;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;

namespace FTN.ServiceContracts.ServiceFabricProxy
{
    public class NetworkModelGDASfProxy : INetworkModelGDAContract
    {

        private ServicePartitionResolver resolver = ServicePartitionResolver.GetDefault();

        private WcfCommunicationClientFactory<INetworkModelGDAContract> factory;
        private ServicePartitionClient<WcfCommunicationClient<INetworkModelGDAContract>> proxy;

        public NetworkModelGDASfProxy()
        {
            factory = new WcfCommunicationClientFactory<INetworkModelGDAContract>(
                    clientBinding: CommonCloud.Binding.CreateCustomNetTcp(),
                    servicePartitionResolver: resolver);

            proxy = new ServicePartitionClient<WcfCommunicationClient<INetworkModelGDAContract>>(
                    communicationClientFactory: factory,
                    serviceUri: new Uri("fabric:/CloudEMS/NetworkModelCloudServ"),
                    listenerName: "NMSEndpoint");
        }
        public UpdateResult ApplyUpdate(Delta delta)
        {
            return proxy.InvokeWithRetry(x => x.Channel.ApplyUpdate(delta));
        }

        public int GetExtentValues(ModelCode entityType, List<ModelCode> propIds)
        {
            return proxy.InvokeWithRetry(x => x.Channel.GetExtentValues(entityType, propIds));
        }

        public int GetRelatedValues(long source, List<ModelCode> propIds, Association association)
        {
            return proxy.InvokeWithRetry(x => x.Channel.GetRelatedValues(source, propIds, association));
        }

        public ResourceDescription GetValues(long resourceId, List<ModelCode> propIds)
        {
            return proxy.InvokeWithRetry(x => x.Channel.GetValues(resourceId, propIds));
        }

        public bool IteratorClose(int id)
        {
            return proxy.InvokeWithRetry(x => x.Channel.IteratorClose(id));
        }

        public List<ResourceDescription> IteratorNext(int n, int id)
        {
            return proxy.InvokeWithRetry(x => x.Channel.IteratorNext(n,id));
        }

        public int IteratorResourcesLeft(int id)
        {
            return proxy.InvokeWithRetry(x => x.Channel.IteratorResourcesLeft(id));
        }

        public int IteratorResourcesTotal(int id)
        {
            return proxy.InvokeWithRetry(x => x.Channel.IteratorResourcesTotal(id));
        }

        public bool IteratorRewind(int id)
        {
            return proxy.InvokeWithRetry(x => x.Channel.IteratorRewind(id));
        }
    }
}
