using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonCloud;
using FTN.ServiceContracts;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;

namespace CEPubSubContract
{
    public class CEPublishProxy : ICEPublishContract
    {
        private ServicePartitionResolver resolver = ServicePartitionResolver.GetDefault();

        private WcfCommunicationClientFactory<ICEPublishContract> factory;
        private ServicePartitionClient<WcfCommunicationClient<ICEPublishContract>> proxy;
        public CEPublishProxy()
        {
            factory = new WcfCommunicationClientFactory<ICEPublishContract>(
                    clientBinding: Binding.CreateCustomNetTcp(),
                    servicePartitionResolver: resolver);

            proxy = new ServicePartitionClient<WcfCommunicationClient<ICEPublishContract>>(
                    communicationClientFactory: factory,
                    serviceUri: new Uri("fabric:/CloudEMS/CalculationEngineCloudServ"),
                    listenerName: "CEPublishEndpoint");
        }

        public void OptimizationResults(List<MeasurementUI> result)
        {
            proxy.InvokeWithRetry(x => x.Channel.OptimizationResults(result));
        }

        public void PublishCoReduction(Tuple<string, float, float> tupla)
        {
            proxy.InvokeWithRetry(x => x.Channel.PublishCoReduction(tupla));
        }

        

        public void RenewableResult(Tuple<DateTime, float> renewableKW)
        {
            proxy.InvokeWithRetry(x => x.Channel.RenewableResult(renewableKW));
        }

        public void WindPercentResult(float result)
        {
            proxy.InvokeWithRetry(x => x.Channel.WindPercentResult(result));
        }
      
    }
}
