using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using CommonCloud;
using FTN.ServiceContracts;
using FTN.Services.AlarmsEventsService;
using FTN.Services.AlarmsEventsService.PubSub;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace AlarmsEventsCloudServ
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class AlarmsEventsCloudServ : StatefulService
    {
        private PublisherService publisherService;

        private AlarmsEvents alarmsEvents;
        public AlarmsEventsCloudServ(StatefulServiceContext context)
            : base(context)
        {
            alarmsEvents = new AlarmsEvents();
            publisherService = new PublisherService();
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new List<ServiceReplicaListener>() {
                new ServiceReplicaListener(context => this.CreateAlarmEventsListener(context), "AlarmsEventsEndpoint"),
                new ServiceReplicaListener(context => this.CreateAlarmsEventsIntegrityListener(context), "AlarmsEventsIntegrityEndpoint"),
                new ServiceReplicaListener(context => this.CreateAlarmsEventsPubSubListener(context), "AlarmsEventsPubSubEndpoint"),
            };
        }

        private ICommunicationListener CreateAlarmEventsListener(StatefulServiceContext context)
        {
            var listener = new WcfCommunicationListener<IAlarmsEventsContract>(
                listenerBinding: Binding.CreateCustomNetTcp(),
                address: new EndpointAddress("net.tcp://localhost:20023/AlarmsEventsCloudServ"),
                serviceContext: context,
                wcfServiceObject: alarmsEvents
            );

            return listener;
        }
        private ICommunicationListener CreateAlarmsEventsIntegrityListener(StatefulServiceContext context)
        {
            var listener = new WcfCommunicationListener<IAesIntegirtyContract>(
                listenerBinding: Binding.CreateCustomNetTcp(),
                address: new EndpointAddress("net.tcp://localhost:20023/AlarmsEventsCloudServ/IntegrityUpdate"),
                serviceContext: context,
                wcfServiceObject: alarmsEvents
            );

            return listener;
        }
        
        private ICommunicationListener CreateAlarmsEventsPubSubListener(StatefulServiceContext context)
        {
            var listener = new WcfCommunicationListener<IAesPubSubContract>(
                listenerBinding: Binding.CreateCustomNetTcp(),
                address: new EndpointAddress("net.tcp://localhost:20023/AlarmsEventsCloudServ/PublisherService"),
                serviceContext: context,
                wcfServiceObject: publisherService
            );

            return listener;
        }
        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            alarmsEvents.Instantiate(this.StateManager);

            ServiceEventSource.Current.ServiceMessage(this.Context, "AES instantiation finished.");

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
