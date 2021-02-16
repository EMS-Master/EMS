using CommonMeas;
using FTN.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace UI.Communication
{
    public class UIAesIntegrityClient : ClientBase<IAesIntegirtyContract>, IAesIntegirtyContract
    {
        public UIAesIntegrityClient() { }
        public UIAesIntegrityClient(string Endpoint) : base(Endpoint) { }

        public List<AlarmHelper> InitiateIntegrityUpdate()
        {
            return Channel.InitiateIntegrityUpdate();
        }
    }
}
