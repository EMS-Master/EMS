using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using AESPubSbuContract;
using CommonCloud;
using FTN.ServiceContracts;
using FTN.Services.AlarmsEventsService.PubSub;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace AesPubSubService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class AesPubSubService : StatelessService
    {
        private AesPubSub aesPubSub;
        public AesPubSubService(StatelessServiceContext context)
            : base(context)
        {
            aesPubSub = new AesPubSub();
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new List<ServiceInstanceListener>
            {
                new ServiceInstanceListener(context => this.CreateAesSubscribeListener(context), "AlarmsEventsPubSubEndpoint"),
                new ServiceInstanceListener(context => this.CreateAesPublishListener(context), "AESPublishEndpoint")

            };
        }

        private ICommunicationListener CreateAesSubscribeListener(StatelessServiceContext context)
        {
            var listener = new WcfCommunicationListener<IAesPubSubContract>(
                listenerBinding: Binding.CreateCustomNetTcp(),
                address: new EndpointAddress("net.tcp://localhost:52396/AesPubSubService"),
                //endpointResourceName: "AlarmsEventsPubSubEndpoint",
                serviceContext: context,
                wcfServiceObject: aesPubSub
            );

            return listener;
        }

        private ICommunicationListener CreateAesPublishListener(StatelessServiceContext context)
        {
            var listener = new WcfCommunicationListener<IAesPublishContract>(
                listenerBinding: Binding.CreateCustomNetTcp(),
                endpointResourceName: "AESPublishEndpoint",
                serviceContext: context,
                wcfServiceObject: aesPubSub
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
