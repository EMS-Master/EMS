using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonMeas;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;

namespace CalculationEngineContracts.ServiceFabricProxy
{
    public class Co2EmissionAndReductionSfProxy : ICo2EmissionAndReduction
    {
        private ServicePartitionResolver resolver = ServicePartitionResolver.GetDefault();

        private WcfCommunicationClientFactory<ICo2EmissionAndReduction> factory;
        private ServicePartitionClient<WcfCommunicationClient<ICo2EmissionAndReduction>> proxy;
        

        public Co2EmissionAndReductionSfProxy()
        {
            factory = new WcfCommunicationClientFactory<ICo2EmissionAndReduction>(
                    clientBinding: CommonCloud.Binding.CreateCustomNetTcp(),
                    servicePartitionResolver: resolver);

            proxy = new ServicePartitionClient<WcfCommunicationClient<ICo2EmissionAndReduction>>(
                    communicationClientFactory: factory,
                    serviceUri: new Uri("fabric:/CloudEMS/Co2EmissionAndReductionService"),
                    listenerName: "Co2EmissionAndReductionEndpoint");
            
        }
        public float CalculateCO2(Dictionary<long, OptimisationModel> optModelMap)
        {
            try
            {
                return proxy.InvokeWithRetry(x => x.Channel.CalculateCO2(optModelMap));
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public float CalculateCO2ReductionWithBiggestCoeficient(Dictionary<long, OptimisationModel> optModelMap)
        {
            try
            {
                return proxy.InvokeWithRetry(x => x.Channel.CalculateCO2ReductionWithBiggestCoeficient(optModelMap));
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public float CalculateCO2WithKyotoProtocol(Dictionary<long, OptimisationModel> optModelMap)
        {
            try
            {
                return proxy.InvokeWithRetry(x => x.Channel.CalculateCO2WithKyotoProtocol(optModelMap));
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
