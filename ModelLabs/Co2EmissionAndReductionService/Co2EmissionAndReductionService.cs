using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CalculationEngineContracts;
using CalculationEngineServ;
using CommonCloud;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Co2EmissionAndReductionService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class Co2EmissionAndReductionService : StatelessService
    {
        private Co2EmissionAndReductionCloud co2;
        public Co2EmissionAndReductionService(StatelessServiceContext context)
            : base(context)
        {
            co2 = new Co2EmissionAndReductionCloud();
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new List<ServiceInstanceListener>
            {
                new ServiceInstanceListener(context => this.CreateCo2EmissionAndReductionListener(context), "Co2EmissionAndReductionEndpoint")
            };
        }

        private ICommunicationListener CreateCo2EmissionAndReductionListener(StatelessServiceContext context)
        {
            var listener = new WcfCommunicationListener<ICo2EmissionAndReduction>(
                listenerBinding: Binding.CreateCustomNetTcp(),
                endpointResourceName: "Co2EmissionAndReductionEndpoint",
                serviceContext: context,
                wcfServiceObject: co2
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

            //long iterations = 0;
            ServiceEventSource.Current.ServiceMessage(this.Context, "Co2EmissionnReduction");


            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                //ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
