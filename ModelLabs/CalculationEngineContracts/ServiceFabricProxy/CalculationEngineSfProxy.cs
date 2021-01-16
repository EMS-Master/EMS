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
    public class CalculationEngineSfProxy : ICalculationEngineContract, ICalculationEngineRepository
    {
        private ServicePartitionResolver resolver = ServicePartitionResolver.GetDefault();

        private WcfCommunicationClientFactory<ICalculationEngineContract> factory;
        private ServicePartitionClient<WcfCommunicationClient<ICalculationEngineContract>> proxy;

        private WcfCommunicationClientFactory<ICalculationEngineRepository> factory2;
        private ServicePartitionClient<WcfCommunicationClient<ICalculationEngineRepository>> proxy2;

        public CalculationEngineSfProxy()
        {
            factory = new WcfCommunicationClientFactory<ICalculationEngineContract>(
                    clientBinding: CommonCloud.Binding.CreateCustomNetTcp(),
                    servicePartitionResolver: resolver);

            proxy = new ServicePartitionClient<WcfCommunicationClient<ICalculationEngineContract>>(
                    communicationClientFactory: factory,
                    serviceUri: new Uri("fabric:/CloudEMS/CalculationEngineCloudServ"),
                    listenerName: "CalculationEngineEndpoint");

            factory2 = new WcfCommunicationClientFactory<ICalculationEngineRepository>(
                    clientBinding: CommonCloud.Binding.CreateCustomNetTcp(),
                    servicePartitionResolver: resolver);

            proxy2 = new ServicePartitionClient<WcfCommunicationClient<ICalculationEngineRepository>>(
                    communicationClientFactory: factory2,
                    serviceUri: new Uri("fabric:/CloudEMS/CalculationEngineCloudServ"),
                    listenerName: "CalculationEngineRepositoryEndpoint");
        }

        public List<DiscreteCounterModel> GetAllDiscreteCounters()
        {
            return proxy2.InvokeWithRetry(x => x.Channel.GetAllDiscreteCounters());
        }

        public Dictionary<Tuple<long, string>, int> GetCounterForGeneratorType()
        {
            return proxy2.InvokeWithRetry(x => x.Channel.GetCounterForGeneratorType());
        }

        public void InsertOrUpdate(DiscreteCounterModel model)
        {
            proxy2.InvokeWithRetry(x => x.Channel.InsertOrUpdate(model));
        }

        public bool OptimisationAlgorithm(List<MeasurementUnit> measEnergyConsumer, List<MeasurementUnit> measGenerators, float windData, float sunData)
        {
            return proxy.InvokeWithRetry(x => x.Channel.OptimisationAlgorithm(measEnergyConsumer, measGenerators, windData, sunData));
        }
    }
}
