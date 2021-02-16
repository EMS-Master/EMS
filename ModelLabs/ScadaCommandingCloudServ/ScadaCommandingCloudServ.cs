using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using ScadaCommandingService;
using ScadaContracts;
using TransactionContract;

namespace ScadaCommandingCloudServ
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class ScadaCommandingCloudServ : StatelessService
    {
        private ScadaCommand scadaCMD;
        public ScadaCommandingCloudServ(StatelessServiceContext context)
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
            //return new ServiceInstanceListener[0];
            return new List<ServiceInstanceListener>
            {
                new ServiceInstanceListener(context => this.CreateScadaCMDListener(context), "ScadaCMDEndpoint"),
                new ServiceInstanceListener(context => this.CreateTransactionListener(context), "TransactionEndpoint"),
                new ServiceInstanceListener(context => this.CreateUICommandListener(context), "UIScadaCommandClientEndpoint")

            };
        }
        private ICommunicationListener CreateScadaCMDListener(StatelessServiceContext context)
        {
            var listener = new WcfCommunicationListener<IScadaCommandingContract>(
                listenerBinding: CommonCloud.Binding.CreateCustomNetTcp(),
                endpointResourceName: "ScadaCMDEndpoint",
                serviceContext: context,
                wcfServiceObject: scadaCMD
            );

            return listener;
        }
        private ICommunicationListener CreateUICommandListener(StatelessServiceContext context)
        {
            var listener = new WcfCommunicationListener<IScadaCommandingContract>(
                listenerBinding: CommonCloud.Binding.CreateCustomNetTcp(),
                //endpointResourceName: "ScadaCMDEndpoint",
                address: new System.ServiceModel.EndpointAddress("net.tcp://localhost:52394/ScadaCommandingCloudServ"),
                serviceContext: context,
                wcfServiceObject: scadaCMD
            );

            return listener;
        }

        private ICommunicationListener CreateTransactionListener(StatelessServiceContext context)
        {
            var listener = new WcfCommunicationListener<ITransactionContract>(
                listenerBinding: CommonCloud.Binding.CreateCustomNetTcp(),
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
            #region ScadaCommanding instantiation
            
            bool integrityState = scadaCMD.InitiateIntegrityUpdate();

            if (!integrityState)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, "CalculationEngine integrity update failed");
            }
            else
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, "CalculationEngine integrity update succeeded.");
            }

            #endregion ScadaCommanding instantiation

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
