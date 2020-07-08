using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using FTN.Common;
using FTN.Services.AlarmsEventsService.PubSub;


namespace FTN.Services.AlarmsEventsService
{
    public class AlarmsEventsService : IDisposable
    {
        private AlarmsEvents ae = null;

        private List<ServiceHost> hosts = null;

        public AlarmsEventsService()
        {
            this.ae = new AlarmsEvents();
            this.InitializeHosts();
        }

        public void Start()
        {
            this.StartHosts();
        }
        public void Dispose()
        {
            this.CloseHosts();
            GC.SuppressFinalize(this);
        }

        public void CloseHosts()
        {
            if (hosts == null || hosts.Count == 0)
            {
                throw new Exception("Alarms Events Service can not be closed because it is not initialized.");
            }

            foreach (ServiceHost host in hosts)
            {
                host.Close();
            }

            string message = "The Alarms Events Service is closed.";
            CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
            Console.WriteLine("\n\n{0}", message);
        }

        private void InitializeHosts()
        {
            hosts = new List<ServiceHost>();
            hosts.Add(new ServiceHost(typeof(AlarmsEvents)));
            hosts.Add(new ServiceHost(typeof(PublisherService)));
          
        }

        private void StartHosts()
        {
            if (hosts == null || hosts.Count == 0)
            {
                throw new Exception("Alarms Events Service can not be opend because it is not initialized.");
            }

            string message = string.Empty;
            foreach (ServiceHost host in hosts)
            {
                host.Open();
                message = string.Format("The WCF service {0} is ready.", host.Description.Name);
                Console.WriteLine(message);
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
                message = "Endpoints:";
                Console.WriteLine(message);
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
                foreach (Uri uri in host.BaseAddresses)
                {
                    Console.WriteLine(uri);
                    CommonTrace.WriteTrace(CommonTrace.TraceInfo, uri.ToString());
                }

                Console.WriteLine("\n");
            }
            CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
            message = string.Format("Trace level: {0}", CommonTrace.TraceLevel);
            Console.WriteLine(message);
            CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
            message = "The Alarms Events Service is started.";
            Console.WriteLine("\n{0}", message);
            CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
        }
    }
}
