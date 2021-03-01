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
                    serviceUri: new Uri("fabric:/CloudEMS/CePubSubService"),
                    listenerName: "CEPublishEndpoint");
        }

        public void OptimizationResults(List<MeasurementUI> result)
        {
            try
            {
                proxy.InvokeWithRetry(x => x.Channel.OptimizationResults(result));

            }
            catch (Exception e)
            {
                throw;
            }
        }

        public void PublishCoReduction(Tuple<string, float, float> tupla)
        {
            try
            {
                proxy.InvokeWithRetry(x => x.Channel.PublishCoReduction(tupla));

            }
            catch (Exception e)
            {

                throw;
            }
        }

        

        public void RenewableResult(Tuple<DateTime, float> renewableKW)
        {
            try
            {
                proxy.InvokeWithRetry(x => x.Channel.RenewableResult(renewableKW));

            }
            catch (Exception e)
            {
                throw;
            }
        }

        public void WindPercentResult(float result)
        {
            try
            {
                proxy.InvokeWithRetry(x => x.Channel.WindPercentResult(result));

            }
            catch (Exception e)
            {
                throw;
            }
        }
      
    }
}
