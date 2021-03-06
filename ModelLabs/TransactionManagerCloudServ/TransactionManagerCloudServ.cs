using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using CommonCloud;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using TransactionContract;
using TransactionManagerService;

namespace TransactionManagerCloudServ
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class TransactionManagerCloudServ : StatelessService
    {
        private TransactionManager transactionManager = null;

        public TransactionManagerCloudServ(StatelessServiceContext context)
            : base(context)
        {
            transactionManager = new TransactionManager();
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new List<ServiceInstanceListener>
            {
                new ServiceInstanceListener(context => this.CreateTransactionManagerListener(context), "TransactionManagerEndpoint"),
                new ServiceInstanceListener(context => this.CreateTransactionManagerListenerImporter(context), "ImporterClientEndpoint"),
                //new ServiceInstanceListener(context => this.CreateServiceRemotingListener(context), "TransactionManagerAsyncEndpoint")
            };
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            //long iterations = 0;
            ServiceEventSource.Current.ServiceMessage(this.Context, "TransactionManagerService");


            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                //ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
        private ICommunicationListener CreateTransactionManagerListener(StatelessServiceContext context)
        {
            var listener = new WcfCommunicationListener<IImporterContract>(
                    listenerBinding: Binding.CreateCustomNetTcp(),
                    address: new EndpointAddress("net.tcp://localhost:50000/TransactionManagerCloudServ/Importer"),
                    serviceContext: context,
                    wcfServiceObject: transactionManager
                );
            ServiceEventSource.Current.ServiceMessage(context, "Created listener for Importer");

            return listener;
        }

        private ICommunicationListener CreateTransactionManagerListenerImporter(StatelessServiceContext context)
        {
            var listener = new WcfCommunicationListener<IImporterContract>(
                    listenerBinding: Binding.CreateCustomNetTcp(),
                    address: new EndpointAddress("net.tcp://localhost:52392/TransactionManagerCloudServ"),
                    serviceContext: context,
                    wcfServiceObject: transactionManager
                );

            ServiceEventSource.Current.ServiceMessage(context, "Created listener for TransactionManagerCloudServ => UI");

            return listener;
        }
    }
}
