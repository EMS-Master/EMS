using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using CloudCommon;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using ScadaCommandingService;
using ScadaContracts;
using TransactionContract;

namespace ScadaCommandingCloudService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class ScadaCommandingCloudService : StatelessService
    {
        private ScadaCommand scadaCMD;

        public ScadaCommandingCloudService(StatelessServiceContext context)
            : base(context)
        {
            scadaCMD = new ScadaCommand();
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new List<ServiceInstanceListener>
            {
                new ServiceInstanceListener(context => this.CreateScadaCMDListener(context), "ScadaCMDEndpoint"),
                new ServiceInstanceListener(context => this.CreateTransactionListener(context), "TransactionEndpoint")
            };
        }

        private ICommunicationListener CreateScadaCMDListener(StatelessServiceContext context)
        {
            var listener = new WcfCommunicationListener<IScadaCommandingContract>(
                listenerBinding: Binding.CreateCustomNetTcp(),
                //address: new EndpointAddress("net.tcp://localhost:34000/ScadaCloudCommanding"),
                endpointResourceName: "ScadaCMDEndpoint",
                serviceContext: context,
                wcfServiceObject: scadaCMD
            );

            return listener;
        }

        private ICommunicationListener CreateTransactionListener(StatelessServiceContext context)
        {
            var listener = new WcfCommunicationListener<ITransactionContract>(
                listenerBinding: Binding.CreateCustomNetTcp(),
                endpointResourceName: "TransactionEndpoint",
                serviceContext: context,
                wcfServiceObject: scadaCMD
            );

            return listener;
        }
        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            long iterations = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
