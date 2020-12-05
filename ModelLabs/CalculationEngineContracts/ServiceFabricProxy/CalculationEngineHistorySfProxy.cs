using CloudCommon;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineContracts.ServiceFabricProxy
{
    public class CalculationEngineHistorySfProxy : ICalculationEngineUIContract
    {
        private ServicePartitionResolver resolver = ServicePartitionResolver.GetDefault();

        private WcfCommunicationClientFactory<ICalculationEngineUIContract> factory;
        private ServicePartitionClient<WcfCommunicationClient<ICalculationEngineUIContract>> proxy;

        public CalculationEngineHistorySfProxy()
        {
            factory = new WcfCommunicationClientFactory<ICalculationEngineUIContract>(
                    clientBinding: Binding.CreateCustomNetTcp(),
                    servicePartitionResolver: resolver);

            proxy = new ServicePartitionClient<WcfCommunicationClient<ICalculationEngineUIContract>>(
                    communicationClientFactory: factory,
                    serviceUri: new Uri("fabric:/EMSServiceFab/CalculationEngineCloudService"),
                    listenerName: "CalculationEngineUIEndpoint",
                    partitionKey: new ServicePartitionKey("HistoryData"));
        }

        public Tuple<int, int, int, float> GetAlgorithmOptions()
        {
            return proxy.InvokeWithRetry(x => x.Channel.GetAlgorithmOptions());
        }

        public List<Tuple<double, DateTime>> GetCoEmission(DateTime startTime, DateTime endTime)
        {
            return proxy.InvokeWithRetry(x => x.Channel.GetCoEmission(startTime, endTime));
        }

        public List<Tuple<double, DateTime>> GetCoReduction(DateTime startTime, DateTime endTime)
        {
            return proxy.InvokeWithRetry(x => x.Channel.GetCoReduction(startTime, endTime));
        }

        public List<Tuple<double, DateTime>> GetCost(DateTime startTime, DateTime endTime)
        {
            return proxy.InvokeWithRetry(x => x.Channel.GetCost(startTime, endTime));
        }

        public List<Tuple<double, DateTime>> GetHistoryMeasurements(long gid, DateTime startTime, DateTime endTime)
        {
            return proxy.InvokeWithRetry(x => x.Channel.GetHistoryMeasurements(gid,startTime, endTime));
        }

        public List<float> GetPointForFuelEconomy(long gid)
        {
            return proxy.InvokeWithRetry(x => x.Channel.GetPointForFuelEconomy(gid));
        }

        public Tuple<float, float, float> GetPricePerGeneratorType()
        {
            return proxy.InvokeWithRetry(x => x.Channel.GetPricePerGeneratorType());
        }

        public List<Tuple<double, DateTime>> GetProfit(DateTime startTime, DateTime endTime)
        {
            return proxy.InvokeWithRetry(x => x.Channel.GetProfit(startTime, endTime));
        }

        public List<Tuple<double, DateTime>> GetTotalProduction(DateTime startTime, DateTime endTime)
        {
            return proxy.InvokeWithRetry(x => x.Channel.GetTotalProduction(startTime, endTime));
        }

        public void ResetCommandedGenerator(long gid)
        {
            proxy.InvokeWithRetry(x => x.Channel.ResetCommandedGenerator(gid));
        }

        public bool SetAlgorithmOptions(int iterationCount, int populationCount, int elitisamPct, float mutationRate)
        {
            return proxy.InvokeWithRetry(x => x.Channel.SetAlgorithmOptions(iterationCount, populationCount, elitisamPct, mutationRate));
        }

        public bool SetAlgorithmOptionsDefault()
        {
            return proxy.InvokeWithRetry(x => x.Channel.SetAlgorithmOptionsDefault());
        }

        public bool SetPricePerGeneratorType(float oilPrice, float coalPrice, float gasPrice)
        {
            return proxy.InvokeWithRetry(x => x.Channel.SetPricePerGeneratorType(oilPrice, coalPrice, gasPrice));
        }

        public bool SetPricePerGeneratorTypeDefault()
        {
            return proxy.InvokeWithRetry(x => x.Channel.SetPricePerGeneratorTypeDefault());
        }
    }
}
