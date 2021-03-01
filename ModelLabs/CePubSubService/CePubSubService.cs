using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using CalculationEngineServ.PubSub;
using CEPubSubContract;
using CommonCloud;
using FTN.ServiceContracts;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace CePubSubService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class CePubSubService : StatelessService
    {
        private CePubSub cePubSub;
        public CePubSubService(StatelessServiceContext context)
            : base(context)
        {
            cePubSub = new CePubSub();
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new List<ServiceInstanceListener>
            {
                new ServiceInstanceListener(context => this.CreateCePublishListener(context), "CEPublishEndpoint"),
                new ServiceInstanceListener(context => this.CreateCESubscribeListener(context), "UISubscribeClientEndpoint")

            };
        }

        private ICommunicationListener CreateCePublishListener(StatelessServiceContext context)
        {
            var listener = new WcfCommunicationListener<ICEPublishContract>(
                           listenerBinding: Binding.CreateCustomNetTcp(),
                           endpointResourceName: "CEPublishEndpoint",
                           serviceContext: context,
                           wcfServiceObject: cePubSub
            );
            ServiceEventSource.Current.ServiceMessage(context, "Created listener for CalculationEngineHistoryDataEndpoint");
            return listener;
        }

        private ICommunicationListener CreateCESubscribeListener(StatelessServiceContext context)
        {
            var listener = new WcfCommunicationListener<ICePubSubContract>(
                           listenerBinding: Binding.CreateCustomNetTcp(),
                           address: new EndpointAddress("net.tcp://localhost:52398/CePubSubService"),
                           //endpointResourceName: "CESubscribeEndpoint",
                           serviceContext: context,
                           wcfServiceObject: cePubSub
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
