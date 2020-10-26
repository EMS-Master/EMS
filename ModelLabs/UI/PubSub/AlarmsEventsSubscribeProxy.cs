using FTN.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace UI.PubSub
{
    public class AlarmsEventsSubscribeProxy : IAesPubSubContract, IDisposable
    {
        private static IAesPubSubContract proxy;
        private static DuplexChannelFactory<IAesPubSubContract> factory;
        private static InstanceContext context;

        public IAesPubSubContract Proxy
        {
            get { return proxy; }
            set { proxy = value; }
        }

        public AlarmsEventsSubscribeProxy(Action<object> callbackAction)
        {
            if (Proxy == null)
            {
                context = new InstanceContext(new AePubSubCallbackService() { CallbackAction = callbackAction });
                factory = new DuplexChannelFactory<IAesPubSubContract>(context, "AlarmsEventsPubSub");
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
    }
}
