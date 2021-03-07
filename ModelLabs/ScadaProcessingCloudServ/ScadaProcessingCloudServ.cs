using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using ScadaContracts;
using ScadaProcessingSevice;
using TransactionContract;

namespace ScadaProcessingCloudServ
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class ScadaProcessingCloudServ : StatefulService
    {
        private ScadaProccessingCloud scadaProcessing;
        public ScadaProcessingCloudServ(StatefulServiceContext context)
            : base(context)
        {
            scadaProcessing = new ScadaProccessingCloud();
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new List<ServiceReplicaListener>
            {
                new ServiceReplicaListener(context => this.CreateScadaCRListener(context), "ScadaPREndpoint"),
                new ServiceReplicaListener(context => this.CreateTransactionPRListener(context), "TransactionPREndpoint")
            };
        }
        private ICommunicationListener CreateScadaCRListener(StatefulServiceContext context)
        {
            var listener = new WcfCommunicationListener<IScadaProcessingContract>(
                listenerBinding: CommonCloud.Binding.CreateCustomNetTcp(),
                endpointResourceName: "ScadaPREndpoint",
                serviceContext: context,
                wcfServiceObject: scadaProcessing
            );

            return listener;
        }

        private ICommunicationListener CreateTransactionPRListener(StatefulServiceContext context)
        {
            var listener = new WcfCommunicationListener<ITransactionContract>(
                listenerBinding: CommonCloud.Binding.CreateCustomNetTcp(),
                endpointResourceName: "TransactionPREndpoint",
                serviceContext: context,
                wcfServiceObject: scadaProcessing
            );

            return listener;
        }
        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            #region ScadaProcessing instantiation
            bool integrityState = scadaProcessing.InitiateIntegrityUpdate();

            if (!integrityState)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, "ScadaProcessing integrity update failed");
            }
            else
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, "ScadaProcessing integrity update succeeded.");
            }

            #endregion ScadaProcessing instantiation

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
