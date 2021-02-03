using FTN.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace UI.PubSub
{
    public class CeSubscribeProxy : ICePubSubContract, IDisposable
    {
        private ICePubSubContract proxy;
        private DuplexChannelFactory<ICePubSubContract> factory;
        private InstanceContext context;

        public ICePubSubContract Proxy
        {
            get { return proxy; }
            set { proxy = value; }
        }
        public CeSubscribeProxy(Action<object> callbackAction)
        {
            if (Proxy == null)
            {
                context = new InstanceContext(new CePubSubCallbackService() { CallbackAction = callbackAction });
                factory = new DuplexChannelFactory<ICePubSubContract>(context, "CalculationEnginePubSub");
                Proxy = factory.CreateChannel();
            }
        }
        public void Dispose()
        {

            if (factory != null)
            {
                factory = null;
            }
        }

        public void Subscribe()
        {
            Proxy.Subscribe();
        }

        public void Unsubscribe()
        {
            Proxy.Unsubscribe();
        }

		public bool Optimization()
		{
            return true;
		}
	}
}
