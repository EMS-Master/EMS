using FTN.Common;
using FTN.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI
{
    public class TestGDA : IDisposable
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        private ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();
        private NetworkModelGDAProxy gdaQueryProxy = null;

        //private NetworkModelGDAProxy GdaQueryProxy
        //{
        //    get
        //    {
        //        if (gdaQueryProxy != null)
        //        {
        //            gdaQueryProxy.Abort();
        //            gdaQueryProxy = null;
        //        }

        //        gdaQueryProxy = new NetworkModelGDAProxy("NetworkModelGDAEndpoint");
        //        gdaQueryProxy.Open();

        //        return gdaQueryProxy;
        //    }
        //}
        public ResourceDescription GetValues(long globalId)
        {
            string message = "Getting values method started.";
            Console.WriteLine(message);
            CommonTrace.WriteTrace(CommonTrace.TraceError, message);

            ResourceDescription rd = null;

            try
            {
                short type = ModelCodeHelper.ExtractTypeFromGlobalId(globalId);
                List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds((DMSType)type);

                //rd = GdaQueryProxy.GetValues(globalId, properties);
                rd = NetworkModelGDAProxy.Instance.GetValues(globalId, properties);

                message = "Getting values method successfully finished.";
                Console.WriteLine(message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, message);
            }
            catch (Exception e)
            {
                message = string.Format("Getting values method for entered id = {0} failed.\n\t{1}", globalId, e.Message);
                Console.WriteLine(message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, message);
            }
            return rd;
        }

        public List<ResourceDescription> GetExtentValues(ModelCode modelCode, List<ModelCode> properties)
        {
            string message = "Getting extent values method started.";
            Console.WriteLine(message);
            CommonTrace.WriteTrace(CommonTrace.TraceError, message);

            int iteratorId = 0;
            List<ResourceDescription> retList = new List<ResourceDescription>();
            try
            {
                int numberOfResources = 2;
                int resourcesLeft = 0;


                if (properties.Count == 0)
                {
                    properties = modelResourcesDesc.GetAllPropertyIds(modelCode);
                }

                //iteratorId = GdaQueryProxy.GetExtentValues(modelCode, properties);
                //resourcesLeft = GdaQueryProxy.IteratorResourcesLeft(iteratorId);

                iteratorId = NetworkModelGDAProxy.Instance.GetExtentValues(modelCode, properties);
                resourcesLeft = NetworkModelGDAProxy.Instance.IteratorResourcesLeft(iteratorId);

                while (resourcesLeft > 0)
                {
                    //List<ResourceDescription> rds = GdaQueryProxy.IteratorNext(numberOfResources, iteratorId);
                    List<ResourceDescription> rds = NetworkModelGDAProxy.Instance.IteratorNext(numberOfResources, iteratorId);
                    retList.AddRange(rds);
                    //resourcesLeft = GdaQueryProxy.IteratorResourcesLeft(iteratorId);
                    resourcesLeft = NetworkModelGDAProxy.Instance.IteratorResourcesLeft(iteratorId);
                }

                //GdaQueryProxy.IteratorClose(iteratorId);
                NetworkModelGDAProxy.Instance.IteratorClose(iteratorId);

                message = "Getting extent values method successfully finished.";
                Console.WriteLine(message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, message);

            }
            catch (Exception e)
            {
                message = string.Format("Getting extent values method failed for {0}.\n\t{1}", modelCode, e.Message);
                Console.WriteLine(message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, message);
            }


            return retList;
        }

        public List<ResourceDescription> GetRelatedValues(long sourceGlobalId, Association association)
        {
            string message = "Getting related values method started.";
            Console.WriteLine(message);
            CommonTrace.WriteTrace(CommonTrace.TraceError, message);

            List<ResourceDescription> resultRds = new List<ResourceDescription>();


            int numberOfResources = 5;
            List<long> ids = new List<long>();
            try
            {


                List<ModelCode> properties = new List<ModelCode>();

                //int iteratorId = GdaQueryProxy.GetRelatedValues(sourceGlobalId, properties, association);
                //int resourcesLeft = GdaQueryProxy.IteratorResourcesLeft(iteratorId);

                int iteratorId = NetworkModelGDAProxy.Instance.GetRelatedValues(sourceGlobalId, properties, association);
                int resourcesLeft = NetworkModelGDAProxy.Instance.IteratorResourcesLeft(iteratorId);


                //import ids
                while (resourcesLeft > 0)
                {
                    //List<ResourceDescription> rds = GdaQueryProxy.IteratorNext(numberOfResources, iteratorId);
                    List<ResourceDescription> rds = NetworkModelGDAProxy.Instance.IteratorNext(numberOfResources, iteratorId);
                    foreach (var rd in rds)
                    {
                        ids.Add(rd.Id);
                    }

                    //resourcesLeft = GdaQueryProxy.IteratorResourcesLeft(iteratorId);
                    resourcesLeft = NetworkModelGDAProxy.Instance.IteratorResourcesLeft(iteratorId);
                }

                //find all properties for each id and call 
                foreach (var id in ids)
                {
                    properties = modelResourcesDesc.GetAllPropertyIdsForEntityId(id);
                    //resultRds.Add(GdaQueryProxy.GetValues(id, properties));
                    resultRds.Add(NetworkModelGDAProxy.Instance.GetValues(id, properties));

                }

                //GdaQueryProxy.IteratorClose(iteratorId);
                NetworkModelGDAProxy.Instance.IteratorClose(iteratorId);

                message = "Getting related values method successfully finished.";
                Console.WriteLine(message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, message);
            }
            catch (Exception e)
            {
                message = string.Format("Getting related values method  failed for sourceGlobalId = {0} and association (propertyId = {1}, type = {2}). Reason: {3}", sourceGlobalId, association.PropertyId, association.Type, e.Message);
                Console.WriteLine(message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, message);
            }


            return resultRds;
        }
    }
}
