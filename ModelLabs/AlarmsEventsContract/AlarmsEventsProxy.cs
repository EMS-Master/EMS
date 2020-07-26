using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using CalculationEngineServ.DataBaseModels;
using CommonMeas;

namespace FTN.ServiceContracts
{
    public class AlarmsEventsProxy : IAlarmsEventsContract, IDisposable
    {
        
        private static IAlarmsEventsContract proxy;

        private static ChannelFactory<IAlarmsEventsContract> factory;

        public static IAlarmsEventsContract Instance
        {
            get
            {
                if (proxy == null)
                {
                    factory = new ChannelFactory<IAlarmsEventsContract>("AlarmsEventsEndpoint");
                    proxy = factory.CreateChannel();
                    IContextChannel cc = proxy as IContextChannel;
                }
                return proxy;
            }

            set
            {
                if (proxy == null)
                {
                    proxy = value;
                }
            }
        }

        public void AddAlarm(Alarm alarm)
        {
            proxy.AddAlarm(alarm);
        }

        public void Dispose()
        {
            try
            {
                if (factory != null)
                {
                    factory = null;
                }
            }
            catch (CommunicationException ce)
            {
                Console.WriteLine("Communication exception: {0}", ce.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("AE proxy exception: {0}", e.Message);
            }
        }

        public void UpdateStatus(Alarm analogLoc, State state)
        {
            proxy.UpdateStatus(analogLoc, state);
        }
    }
}
 