using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CloudCommon;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using ScadaContracts;
using ScadaProcessingSevice;
using TransactionContract;

namespace ScadaProcessingCloudService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class ScadaProcessingCloudService : StatelessService
    {
        private ScadaProcessing scadaPR;
        public ScadaProcessingCloudService(StatelessServiceContext context)
            : base(context)
        {
            scadaPR = new ScadaProcessing();
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new List<ServiceInstanceListener>
            {
                new ServiceInstanceListener(context => this.CreateScadaPRListener(context), "ScadaPREndpoint"),
                new ServiceInstanceListener(context => this.CreateTransactionPRListener(context), "TransactionPREndpoint")
            };
        }

        private ICommunicationListener CreateScadaPRListener(StatelessServiceContext context)
        {
            var listener = new WcfCommunicationListener<IScadaProcessingContract>(
                listenerBinding: Binding.CreateCustomNetTcp(),
                endpointResourceName: "ScadaPREndpoint",
                serviceContext: context,
                wcfServiceObject: scadaPR
            );

            return listener;
        }

        private ICommunicationListener CreateTransactionPRListener(StatelessServiceContext context)
        {
            var listener = new WcfCommunicationListener<ITransactionContract>(
                listenerBinding: Binding.CreateCustomNetTcp(),
                endpointResourceName: "TransactionPREndpoint",
                serviceContext: context,
                wcfServiceObject: scadaPR
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
