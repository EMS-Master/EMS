using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using AESPubSbuContract;
using CommonCloud;
using CommonMeas;
using FTN.Common;
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
        //private AesPubSub publisherService;

        private AlarmsEvents alarmsEvents;
        public AlarmsEventsCloudServ(StatefulServiceContext context)
            : base(context)
        {
            alarmsEvents = new AlarmsEvents();
            //publisherService = new AesPubSub();
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
                new ServiceReplicaListener(context => this.CreateAlarmEventsListener1(context), "UIAlarmUpdateClientEndpoint"),
                new ServiceReplicaListener(context => this.CreateAlarmEventsListener(context), "AlarmsEventsEndpoint"),
                new ServiceReplicaListener(context => this.CreateAlarmsEventsIntegrityListener(context), "AlarmsEventsIntegrityEndpoint"),
                new ServiceReplicaListener(context => this.CreateAlarmsEventsIntegrityClientListener(context), "UIAesIntegrityClientEndpoint"),
                //new ServiceReplicaListener(context => this.CreateAlarmsEventsPubSubListener(context), "UIAlarmsSubscribeClientEndpoint"),
                //new ServiceReplicaListener(context => this.CreateAlarmsEventsPublishListener(context), "AESPublishEndpoint")
            };
        }

        private ICommunicationListener CreateAlarmEventsListener(StatefulServiceContext context)
        {
            var listener = new WcfCommunicationListener<IAlarmsEventsContract>(
                listenerBinding: Binding.CreateCustomNetTcp(),
                //address : new EndpointAddress("net.tcp://localhost:52395/AlarmsEventsCloudServ"),
                endpointResourceName: "AlarmsEventsEndpoint",
                serviceContext: context,
                wcfServiceObject: alarmsEvents
            );

            return listener;
        }

        private ICommunicationListener CreateAlarmEventsListener1(StatefulServiceContext context)
        {
            var listener = new WcfCommunicationListener<IAlarmsEventsContract>(
                listenerBinding: Binding.CreateCustomNetTcp(),
                address: new EndpointAddress("net.tcp://localhost:52395/AlarmsEventsCloudServ"),
                //endpointResourceName: "AlarmsEventsEndpoint",
                serviceContext: context,
                wcfServiceObject: alarmsEvents
            );

            return listener;
        }
        private ICommunicationListener CreateAlarmsEventsIntegrityListener(StatefulServiceContext context)
        {
            var listener = new WcfCommunicationListener<IAesIntegirtyContract>(
                listenerBinding: Binding.CreateCustomNetTcp(),
                endpointResourceName: "AlarmsEventsIntegrityEndpoint",
                serviceContext: context,
                wcfServiceObject: alarmsEvents
            );

            return listener;
        }

        private ICommunicationListener CreateAlarmsEventsIntegrityClientListener(StatefulServiceContext context)
        {
            var listener = new WcfCommunicationListener<IAesIntegirtyContract>(
                listenerBinding: Binding.CreateCustomNetTcp(),
                //endpointResourceName: "AlarmsEventsIntegrityEndpoint",
                address: new EndpointAddress("net.tcp://localhost:52393/AlarmsEventsCloudServ"),
                serviceContext: context,
                wcfServiceObject: alarmsEvents
            );

            return listener;
        }

        //private ICommunicationListener CreateAlarmsEventsPubSubListener(StatefulServiceContext context)
        //{
        //    var listener = new WcfCommunicationListener<IAesPubSubContract>(
        //        listenerBinding: Binding.CreateCustomNetTcp(),
        //        address: new EndpointAddress("net.tcp://localhost:52396/AlarmsEventsCloudServ"),
        //        //endpointResourceName: "AlarmsEventsPubSubEndpoint",
        //        serviceContext: context,
        //        wcfServiceObject: this
        //    );

        //    return listener;
        //}

        //private ICommunicationListener CreateAlarmsEventsPublishListener(StatefulServiceContext context)
        //{
        //    var listener = new WcfCommunicationListener<IAesPublishContract>(
        //        listenerBinding: Binding.CreateCustomNetTcp(),
        //        endpointResourceName: "AESPublishEndpoint",
        //        serviceContext: context,
        //        wcfServiceObject: this
        //    );

        //    return listener;
        //}

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            //alarmEventHandler = new AlarmEventHandler(AlarmsEventsHandler);
            //AlarmEvent += alarmEventHandler;

            //alarmUpdateHandler = new AlarmUpdateHandler(AlarmsUpdateEventsHandler);
            //AlarmUpdate += alarmUpdateHandler;
            //alarmsEvents.Instantiate(this.StateManager);

            //ServiceEventSource.Current.ServiceMessage(this.Context, "AES instantiation finished.");

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }


        //public delegate void AlarmEventHandler(object sender, AlarmsEventsEventArgs e);

        //public delegate void AlarmUpdateHandler(object sender, AlarmUpdateEventArgs e);

        //public static event AlarmEventHandler AlarmEvent;

        //public static event AlarmUpdateHandler AlarmUpdate;

        //private IAesPubSubCallbackContract callback = null;
        //private AlarmEventHandler alarmEventHandler = null;
        //private AlarmUpdateHandler alarmUpdateHandler = null;

        //private static List<IAesPubSubCallbackContract> clientsToPublish = new List<IAesPubSubCallbackContract>(4);

        //private object clientsLocker = new object();


        //public void Subscribe()
        //{
        //    callback = OperationContext.Current.GetCallbackChannel<IAesPubSubCallbackContract>();

        //    clientsToPublish.Add(callback);
        //}

        //public void Unsubscribe()
        //{
        //    callback = OperationContext.Current.GetCallbackChannel<IAesPubSubCallbackContract>();

        //    clientsToPublish.Remove(callback);
        //}

        //public void AlarmsEventsHandler(object sender, AlarmsEventsEventArgs e)
        //{
        //    List<IAesPubSubCallbackContract> faultetClients = new List<IAesPubSubCallbackContract>(4);

        //    foreach (IAesPubSubCallbackContract client in clientsToPublish)
        //    {
        //        if ((client as ICommunicationObject).State.Equals(CommunicationState.Opened))
        //        {
        //            client.AlarmsEvents(e.Alarm);
        //        }
        //        else
        //        {
        //            faultetClients.Add(client);
        //        }
        //    }
        //    lock (clientsLocker)
        //    {
        //        foreach (IAesPubSubCallbackContract client in faultetClients)
        //        {
        //            clientsToPublish.Remove(client);
        //        }
        //    }
        //}

        //public void AlarmsUpdateEventsHandler(object sender, AlarmUpdateEventArgs e)
        //{
        //    List<IAesPubSubCallbackContract> faultetClients = new List<IAesPubSubCallbackContract>(4);

        //    foreach (IAesPubSubCallbackContract client in clientsToPublish)
        //    {
        //        if ((client as ICommunicationObject).State.Equals(CommunicationState.Opened))
        //        {
        //            client.UpdateAlarmsEvents(e.Alarm);
        //        }
        //        else
        //        {
        //            faultetClients.Add(client);
        //        }
        //    }
        //    lock (clientsLocker)
        //    {
        //        foreach (IAesPubSubCallbackContract client in faultetClients)
        //        {
        //            clientsToPublish.Remove(client);
        //        }
        //    }
        //}

        //public void PublishAlarmsEvents(AlarmHelper alarm, PublishingStatus status)
        //{
        //    switch (status)
        //    {
        //        case PublishingStatus.INSERT:
        //            {
        //                AlarmsEventsEventArgs e = new AlarmsEventsEventArgs()
        //                {
        //                    Alarm = alarm
        //                };

        //                try
        //                {
        //                    AlarmEvent(this, e);
        //                }
        //                catch (Exception ex)
        //                {
        //                    string message = string.Format("AES does not have any subscribed client for publishing new alarms. {0}", ex.Message);
        //                    CommonTrace.WriteTrace(CommonTrace.TraceVerbose, message);
        //                    Console.WriteLine(message);
        //                }

        //                break;
        //            }

        //        case PublishingStatus.UPDATE:
        //            {
        //                alarm.PubStatus = PublishingStatus.UPDATE;
        //                AlarmUpdateEventArgs e = new AlarmUpdateEventArgs()
        //                {
        //                    Alarm = alarm
        //                };

        //                try
        //                {
        //                    AlarmUpdate(this, e);
        //                }
        //                catch (Exception ex)
        //                {
        //                    string message = string.Format("AES does not have any subscribed client for publishing alarm status change. {0}", ex.Message);
        //                    CommonTrace.WriteTrace(CommonTrace.TraceVerbose, message);
        //                    Console.WriteLine(message);
        //                }
        //                break;
        //            }
        //    }
        //}


        //public void PublishStateChange(AlarmHelper alarm)
        //{
        //    AlarmUpdateEventArgs e = new AlarmUpdateEventArgs()
        //    {
        //        Alarm = alarm
        //    };

        //    try
        //    {
        //        AlarmUpdate(this, e);
        //    }
        //    catch (Exception ex)
        //    {
        //        string message = string.Format("AES does not have any subscribed client for publishing alarm status change. {0}", ex.Message);
        //        CommonTrace.WriteTrace(CommonTrace.TraceVerbose, message);
        //        Console.WriteLine(message);
        //    }
        //}

        
    }
}
