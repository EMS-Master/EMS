using FTN.Common;
using FTN.ServiceContracts;
using FTN.ServiceContracts.ServiceFabricProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GatewayService
{
    public class NetworkModelGDA : INetworkModelGDAContract
    {
        public UpdateResult ApplyUpdate(Delta delta)
        {
            NetworkModelGDASfProxy networkModelGDASfProxy = new NetworkModelGDASfProxy();
            //ServiceEventSource.Current.Message("NetworkModel GDA - ApplyUpdate");
            return networkModelGDASfProxy.ApplyUpdate(delta);
        }

        public int GetExtentValues(ModelCode entityType, List<ModelCode> propIds)
        {
            NetworkModelGDASfProxy networkModelGDASfProxy = new NetworkModelGDASfProxy();
            //ServiceEventSource.Current.Message("NetworkModel GDA - GetExtentValues");
            return networkModelGDASfProxy.GetExtentValues(entityType, propIds);
        }

        public int GetRelatedValues(long source, List<ModelCode> propIds, Association association)
        {
            NetworkModelGDASfProxy networkModelGDASfProxy = new NetworkModelGDASfProxy();
            //ServiceEventSource.Current.Message("NetworkModel GDA - GetRelatedValues");
            return networkModelGDASfProxy.GetRelatedValues(source, propIds, association);
        }

        public ResourceDescription GetValues(long resourceId, List<ModelCode> propIds)
        {
            NetworkModelGDASfProxy networkModelGDASfProxy = new NetworkModelGDASfProxy();
            //ServiceEventSource.Current.Message("NetworkModel GDA - GetValues");
            return networkModelGDASfProxy.GetValues(resourceId, propIds);
        }

        public bool IteratorClose(int id)
        {
            NetworkModelGDASfProxy networkModelGDASfProxy = new NetworkModelGDASfProxy();
            //ServiceEventSource.Current.Message("NetworkModel GDA - IteratorClose");
            return networkModelGDASfProxy.IteratorClose(id);
        }

        public List<ResourceDescription> IteratorNext(int n, int id)
        {
            NetworkModelGDASfProxy networkModelGDASfProxy = new NetworkModelGDASfProxy();
            //ServiceEventSource.Current.Message("NetworkModel GDA - IteratorNext");
            return networkModelGDASfProxy.IteratorNext(n, id);
        }

        public int IteratorResourcesLeft(int id)
        {
            NetworkModelGDASfProxy networkModelGDASfProxy = new NetworkModelGDASfProxy();
            //ServiceEventSource.Current.Message("NetworkModel GDA - IteratorResourcesLeft");
            return networkModelGDASfProxy.IteratorResourcesLeft(id);
        }

        public int IteratorResourcesTotal(int id)
        {
            NetworkModelGDASfProxy networkModelGDASfProxy = new NetworkModelGDASfProxy();
            //ServiceEventSource.Current.Message("NetworkModel GDA - IteratorResourcesTotal");
            return networkModelGDASfProxy.IteratorResourcesTotal(id);
        }

        public bool IteratorRewind(int id)
        {
            NetworkModelGDASfProxy networkModelGDASfProxy = new NetworkModelGDASfProxy();
            //ServiceEventSource.Current.Message("NetworkModel GDA - IteratorRewind");
            return networkModelGDASfProxy.IteratorRewind(id);
        }
    }
}
