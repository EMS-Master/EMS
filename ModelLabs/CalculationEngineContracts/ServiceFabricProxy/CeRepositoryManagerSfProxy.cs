using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using CommonCloud.AzureStorage.Entities;
using CommonCloud.AzureStorage.Helpers;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;

namespace CalculationEngineContracts.ServiceFabricProxy
{
    public class CeRepositoryManagerSfProxy : ICeRepositoryManager
    {
        private ServicePartitionResolver resolver = ServicePartitionResolver.GetDefault();

        private WcfCommunicationClientFactory<ICeRepositoryManager> factory;
        private ServicePartitionClient<WcfCommunicationClient<ICeRepositoryManager>> proxy;

        public CeRepositoryManagerSfProxy()
        {
            factory = new WcfCommunicationClientFactory<ICeRepositoryManager>(
                    clientBinding: CommonCloud.Binding.CreateCustomNetTcp(),
                    servicePartitionResolver: resolver);

            proxy = new ServicePartitionClient<WcfCommunicationClient<ICeRepositoryManager>>(
                    communicationClientFactory: factory,
                    serviceUri: new Uri("fabric:/CloudEMS/CeRepositoryManagerService"),
                    listenerName: "CeRepositoryManagerEndpoint");
        }

        public void AddCommandedGenerator(CommandedGeneratorHelper commandedGenerator)
        {
            try
            {
                proxy.InvokeWithRetry(x => x.Channel.AddCommandedGenerator(commandedGenerator));
            }
            catch (Exception e)
            {

                throw;
            }
        }

        public void AddHistoryMeasurement(HistoryMeasurementHelper historyMeasurement)
        {
            try
            {
                proxy.InvokeWithRetry(x => x.Channel.AddHistoryMeasurement(historyMeasurement));

            }
            catch (Exception e)
            {

                throw;
            }
        }

        public void AddListCommandedGenerators(List<CommandedGeneratorHelper> commandedGenerators)
        {
            try
            {
                proxy.InvokeWithRetry(x => x.Channel.AddListCommandedGenerators(commandedGenerators));
            }
            catch (Exception e)
            {

                throw;
            }
        }

        public void AddTotalProduction(TotalProductionHelper totalProduction)
        {
            try
            {
                proxy.InvokeWithRetry(x => x.Channel.AddTotalProduction(totalProduction));

            }
            catch (Exception e)
            {

                throw;
            }
        }

        public List<HistoryMeasurementHelper> GetAllHistoryMeasurementsForSelectedTime(DateTime startTime, DateTime endTime, long gid)
        {
            try
            {
                return proxy.InvokeWithRetry(x => x.Channel.GetAllHistoryMeasurementsForSelectedTime(startTime, endTime, gid));
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public CommandedGeneratorHelper GetCommandedGenerator(long gid)
        {
            try
            {
                return proxy.InvokeWithRetry(x => x.Channel.GetCommandedGenerator(gid));
            }
            catch (Exception e)
            {

                throw;
            }
        }

        public List<CommandedGeneratorHelper> GetCommandedGenerators()
        {
            try
            {
                return proxy.InvokeWithRetry(x => x.Channel.GetCommandedGenerators());

            }
            catch (Exception e)
            {

                throw;
            }
        }

        public List<long> GetGidsForCommandedGenerators()
        {
            try
            {
                return proxy.InvokeWithRetry(x => x.Channel.GetGidsForCommandedGenerators());
            }
            catch (Exception e)
            {

                throw;
            }
        }

        public List<TotalProductionHelper> GetTotalProductionsForSelectedTime(DateTime startTime, DateTime endTime)
        {
            try
            {
                return proxy.InvokeWithRetry(x => x.Channel.GetTotalProductionsForSelectedTime(startTime, endTime));
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
