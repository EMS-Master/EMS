using FTN.Common;
using FTN.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineServ
{
    public class CalculationEngineServiceClass : IDisposable
    {
        private CalculationEngine ce = null;
        private List<ServiceHost> hosts = null;

        public CalculationEngineServiceClass()
        {
            this.ce = new CalculationEngine();
            ProcessingToCalculation.CalculationEngine = this.ce;
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

        private void InitializeHosts()
        {
            this.hosts = new List<ServiceHost>();
            this.hosts.Add(new ServiceHost(typeof(ProcessingToCalculation)));
            this.hosts.Add(new ServiceHost(typeof(CalculationEngine)));
        }

        private void StartHosts()
        {
            if (this.hosts == null || this.hosts.Count == 0)
            {
                throw new Exception("Calculation Engine Services can not be opend because it is not initialized.");
            }

            string message = string.Empty;
            foreach (ServiceHost host in this.hosts)
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
            message = "The Calculation Engine Service is started.";
            Console.WriteLine("\n{0}", message);
            CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
        }

        public void CloseHosts()
        {
            if (this.hosts == null || this.hosts.Count == 0)
            {
                throw new Exception("Calculation Engine Services can not be closed because it is not initialized.");
            }

            foreach (ServiceHost host in this.hosts)
            {
                host.Close();
            }

            string message = "The Calculation Engine Service is closed.";
            CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
            Console.WriteLine("\n\n{0}", message);
        }
        
    }
}
