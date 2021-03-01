using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonMeas;
using FTN.Common;
using FTN.Services.NetworkModelService.DataModel.Wires;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;

namespace CalculationEngineContracts.ServiceFabricProxy
{
    public class CostAndProfitCalculationSfProxy : ICostAndProfitCalculation
    {
        private ServicePartitionResolver resolver = ServicePartitionResolver.GetDefault();

        private WcfCommunicationClientFactory<ICostAndProfitCalculation> factory;
        private ServicePartitionClient<WcfCommunicationClient<ICostAndProfitCalculation>> proxy;

        public CostAndProfitCalculationSfProxy()
        {
            factory = new WcfCommunicationClientFactory<ICostAndProfitCalculation>(
                    clientBinding: CommonCloud.Binding.CreateCustomNetTcp(),
                    servicePartitionResolver: resolver);

            proxy = new ServicePartitionClient<WcfCommunicationClient<ICostAndProfitCalculation>>(
                    communicationClientFactory: factory,
                    serviceUri: new Uri("fabric:/CloudEMS/CostAndProfitClaculationService"),
                    listenerName: "CostAndProfitCalculationEndpoint");
        }
        public float CalculateProfit(Dictionary<long, OptimisationModel> allGenerators, IDictionary<long, Generator> generators, Dictionary<GeneratorType, float> allTypes, List<GeneratorCurveModel> generatorCurves)
        {
            try
            {
                return proxy.InvokeWithRetry(x => x.Channel.CalculateProfit(allGenerators,  generators,  allTypes,  generatorCurves));
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public float CalculateTotalCostWhenNecessaryEnergyIsZero(Dictionary<long, OptimisationModel> optModelMap, float TotalCost)
        {
            try
            {
                return proxy.InvokeWithRetry(x => x.Channel.CalculateTotalCostWhenNecessaryEnergyIsZero(optModelMap, TotalCost));
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
