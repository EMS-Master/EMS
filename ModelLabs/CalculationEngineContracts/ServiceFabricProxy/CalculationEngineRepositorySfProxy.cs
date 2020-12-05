using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudCommon;
using CommonMeas;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;

namespace CalculationEngineContracts.ServiceFabricProxy
{
    class CalculationEngineRepositorySfProxy : ICalculationEngineRepository
    {
        private ServicePartitionResolver resolver = ServicePartitionResolver.GetDefault();

        private WcfCommunicationClientFactory<ICalculationEngineRepository> factory;
        private ServicePartitionClient<WcfCommunicationClient<ICalculationEngineRepository>> proxy;

        public CalculationEngineRepositorySfProxy()
        {
            factory = new WcfCommunicationClientFactory<ICalculationEngineRepository>(
                    clientBinding: Binding.CreateCustomNetTcp(),
                    servicePartitionResolver: resolver);

            proxy = new ServicePartitionClient<WcfCommunicationClient<ICalculationEngineRepository>>(
                    communicationClientFactory: factory,
                    serviceUri: new Uri("fabric:/EMSServiceFab/CalculationEngineCloudService"),
                    listenerName: "CalculationEngineRepositoryEndpoint",
                    partitionKey: new ServicePartitionKey("Repository"));
        }

        public List<DiscreteCounterModel> GetAllDiscreteCounters()
        {
            return proxy.InvokeWithRetry(x => x.Channel.GetAllDiscreteCounters());
        }

        public void InsertOrUpdate(DiscreteCounterModel model)
        {
            proxy.InvokeWithRetry(x => x.Channel.InsertOrUpdate(model));
        }
    }
}
