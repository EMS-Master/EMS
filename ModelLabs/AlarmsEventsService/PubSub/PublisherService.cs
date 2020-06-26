using CommonMeas;
using FTN.Common;
using FTN.ServiceContracts;
using FTN.Services.AlarmsEventsService.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace FTN.Services.AlarmsEventsService.PubSub
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class PublisherService : IAesPubSubContract
    {
        public delegate void AlarmEventHandler(object sender, AlarmsEventsEventArgs e);

        public delegate void AlarmUpdateHandler(object sender, AlarmUpdateEventArgs e);

        public static event AlarmEventHandler AlarmEvent;

        public static event AlarmUpdateHandler AlarmUpdate;

        private IAesPubSubCallbackContract callback = null;
        private AlarmEventHandler alarmEventHandler = null;
        private AlarmUpdateHandler alarmUpdateHandler = null;

        private static List<IAesPubSubCallbackContract> clientsToPublish = new List<IAesPubSubCallbackContract>(4);

        private object clientsLocker = new object();

        public PublisherService()
        {
            alarmEventHandler = new AlarmEventHandler(AlarmsEventsHandler);
            AlarmEvent += alarmEventHandler;

            alarmUpdateHandler = new AlarmUpdateHandler(AlarmsUpdateEventsHandler);
            AlarmUpdate += alarmUpdateHandler;
        }

        public void Subscribe()
        {
            callback = OperationContext.Current.GetCallbackChannel<IAesPubSubCallbackContract>();

            clientsToPublish.Add(callback);
        }

        public void Unsubscribe()
        {
            callback = OperationContext.Current.GetCallbackChannel<IAesPubSubCallbackContract>();

            clientsToPublish.Remove(callback);
        }

        public void AlarmsEventsHandler(object sender, AlarmsEventsEventArgs e)
        {
            List<IAesPubSubCallbackContract> faultetClients = new List<IAesPubSubCallbackContract>(4);

            foreach (IAesPubSubCallbackContract client in clientsToPublish) 
            {
                if((client as ICommunicationObject).State.Equals(CommunicationState.Opened))
                {
                    client.AlarmsEvents(e.Alarm);
                }
                else
                {
                    faultetClients.Add(client);
                }
            }
            lock (clientsLocker)
            {
                foreach(IAesPubSubCallbackContract client in faultetClients)
                {
                    clientsToPublish.Remove(client);
                }
            }
        }

        public void AlarmsUpdateEventsHandler(object sender, AlarmUpdateEventArgs e)
        {
            List<IAesPubSubCallbackContract> faultetClients = new List<IAesPubSubCallbackContract>(4);

            foreach (IAesPubSubCallbackContract client in clientsToPublish)
            {
                if ((client as ICommunicationObject).State.Equals(CommunicationState.Opened))
                {
                    client.UpdateAlarmsEvents(e.Alarm);
                }
                else
                {
                    faultetClients.Add(client);
                }
            }
            lock (clientsLocker)
            {
                foreach (IAesPubSubCallbackContract client in faultetClients)
                {
                    clientsToPublish.Remove(client);
                }
            }
        }

        public void PublishAlarmsEvents(AlarmHelper alarm, PublishingStatus status)
        {
            switch (status)
            {
                case PublishingStatus.INSERT:
                    {
                        AlarmsEventsEventArgs e = new AlarmsEventsEventArgs()
                        {
                            Alarm = alarm
                        };

                        try
                        {
                            AlarmEvent(this, e);
                        }
                        catch (Exception ex)
                        {
                            string message = string.Format("AES does not have any subscribed client for publishing new alarms. {0}", ex.Message);
                            CommonTrace.WriteTrace(CommonTrace.TraceVerbose, message);
                            Console.WriteLine(message);
                        }

                        break;
                    }

                case PublishingStatus.UPDATE:
                    {
                        alarm.PubStatus = PublishingStatus.UPDATE;
                        AlarmUpdateEventArgs e = new AlarmUpdateEventArgs()
                        {
                            Alarm = alarm
                        };

                        try
                        {
                            AlarmUpdate(this, e);
                        }
                        catch (Exception ex)
                        {
                            string message = string.Format("AES does not have any subscribed client for publishing alarm status change. {0}", ex.Message);
                            CommonTrace.WriteTrace(CommonTrace.TraceVerbose, message);
                            Console.WriteLine(message);
                        }
                        break;
                    }
            }
        }


        public void PublishStateChange(AlarmHelper alarm)
        {
            AlarmUpdateEventArgs e = new AlarmUpdateEventArgs()
            {
                Alarm = alarm
            };

            try
            {
                AlarmUpdate(this, e);
            }
            catch (Exception ex)
            {
                string message = string.Format("AES does not have any subscribed client for publishing alarm status change. {0}", ex.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceVerbose, message);
                Console.WriteLine(message);
            }
        }

    }
}
