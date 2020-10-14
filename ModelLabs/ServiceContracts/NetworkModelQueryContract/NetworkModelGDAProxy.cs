using System.Collections.Generic;
using FTN.Common;
using System.ServiceModel;
using System;

namespace FTN.ServiceContracts
{
	public class NetworkModelGDAProxy :  INetworkModelGDAContract, IDisposable
    {
        private static INetworkModelGDAContract proxy;
        private static ChannelFactory<INetworkModelGDAContract> factory;
        private static object lockObj = new object();

        public static INetworkModelGDAContract Instance
        {
            get
            {
                lock (lockObj)
                {
                    if (proxy == null)
                    {
                        factory = new ChannelFactory<INetworkModelGDAContract>("*");
                        proxy = factory.CreateChannel();
                        IContextChannel cc = proxy as IContextChannel;
                    }

                    return proxy;
                }
            }

            set
            {
                //lock (lockObj)
                //{
                    if (proxy == null)
                    {
                        proxy = value;
                    }
                //}
            }
        }

        public void Dispose()
        {
            try
            {
                if (factory != null)
                {
                    factory = null;
                }
            }
            catch (CommunicationException ce)
            {
                Console.WriteLine("Communication exception: {0}", ce.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("GDA proxy exception: {0}", e.Message);
            }
        }

  //      public NetworkModelGDAProxy(string endpointName)
		//	: base(endpointName)
		//{
		//}

		public UpdateResult ApplyUpdate(Delta delta)
		{
			return proxy.ApplyUpdate(delta);
		}

		public ResourceDescription GetValues(long resourceId, List<ModelCode> propIds)
		{
			return proxy.GetValues(resourceId, propIds);
		}

		public int GetExtentValues(ModelCode entityType, List<ModelCode> propIds)
		{
			return proxy.GetExtentValues(entityType, propIds); 
		}	

		public int GetRelatedValues(long source, List<ModelCode> propIds, Association association)
		{
			return proxy.GetRelatedValues(source, propIds, association);
		}

		
		public bool IteratorClose(int id)
		{
			return proxy.IteratorClose(id);
		}

		public List<ResourceDescription> IteratorNext(int n, int id)
		{
			return proxy.IteratorNext(n, id);
		}

		public int IteratorResourcesLeft(int id)
		{
			return proxy.IteratorResourcesLeft(id);
		}

		public int IteratorResourcesTotal(int id)
		{
			return proxy.IteratorResourcesTotal(id);
		}

		public bool IteratorRewind(int id)
		{
			return proxy.IteratorRewind(id);
		}
	}
}
