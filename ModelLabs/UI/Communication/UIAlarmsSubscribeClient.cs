using FTN.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace UI.Communication
{
    public class UIAlarmsSubscribeClient : ClientBase<IAesPubSubContract>, IAesPubSubContract
    {
        public UIAlarmsSubscribeClient() { }
        public UIAlarmsSubscribeClient(string Endpoint) : base(Endpoint) { }
        public UIAlarmsSubscribeClient(InstanceContext callbackInstance, string endpointConfigurationName) : base(callbackInstance, endpointConfigurationName) { }
        public void Subscribe()
        {
            Channel.Subscribe();
        }

        public void Unsubscribe()
        {
            Channel.Unsubscribe();
        }
    }

}
