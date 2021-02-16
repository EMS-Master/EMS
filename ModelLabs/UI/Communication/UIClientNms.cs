using FTN.Common;
using FTN.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace UI.Communication
{
    public class UIClientNms : ClientBase<INetworkModelGDAContract>, INetworkModelGDAContract
    {
        public UIClientNms() { }
        public UIClientNms(string Endpoint) : base(Endpoint) { }
        public UpdateResult ApplyUpdate(Delta delta)
        {
            return Channel.ApplyUpdate(delta);
        }

        public int GetExtentValues(ModelCode entityType, List<ModelCode> propIds)
        {
            return Channel.GetExtentValues(entityType, propIds);
        }

        public int GetRelatedValues(long source, List<ModelCode> propIds, Association association)
        {
            return Channel.GetRelatedValues(source, propIds, association);
        }

        public ResourceDescription GetValues(long resourceId, List<ModelCode> propIds)
        {
            return Channel.GetValues(resourceId, propIds);
        }

        public bool IteratorClose(int id)
        {
            return Channel.IteratorClose(id);
        }

        public List<ResourceDescription> IteratorNext(int n, int id)
        {
            return Channel.IteratorNext(n, id);
        }

        public int IteratorResourcesLeft(int id)
        {
            return Channel.IteratorResourcesLeft(id);
        }

        public int IteratorResourcesTotal(int id)
        {
            return Channel.IteratorResourcesTotal(id);
        }

        public bool IteratorRewind(int id)
        {
            return Channel.IteratorRewind(id);
        }
    }
}
