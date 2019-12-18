using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ScadaProcessingSevice
{
    public class ScadaProcSevice : IDisposable
    {
        private ScadaProcessing scadaProcessing = null;
        private List<ServiceHost> hosts = null;

        public ScadaProcSevice()
        {
            this.scadaProcessing = new ScadaProcessing();
            this.InitializeHosts();
        }

        public void Start()
        {
            this.StartHosts();
        }

        public void CloseHosts()
        {
            if (this.hosts == null || this.hosts.Count == 0)
            {
                throw new Exception("SCADA Processing Services can not be closed because it is not initialized.");
            }

            foreach (ServiceHost host in this.hosts)
            {
                host.Close();
            }

            string message = "The SCADA Processing Service is closed.";
            Console.WriteLine("\n\n{0}", message);
        }

        public void Dispose()
        {
            this.CloseHosts();
            GC.SuppressFinalize(this);
        }

        private void InitializeHosts()
        {
            this.hosts = new List<ServiceHost>();
            this.hosts.Add(new ServiceHost(typeof(ScadaProcessing)));
        }

        private void StartHosts()
        {
            if (this.hosts == null || this.hosts.Count == 0)
            {
                throw new Exception("SCADA Processing Services can not be opend because it is not initialized.");
            }

            string message = string.Empty;
            foreach (ServiceHost host in this.hosts)
            {
                try
                {
                    host.Open();

                    message = string.Format("The WCF service {0} is ready.", host.Description.Name);
                    Console.WriteLine(message);

                    foreach (Uri uri in host.BaseAddresses)
                    {
                        Console.WriteLine(uri);
                    }

                    Console.WriteLine("\n");
                }
                catch (CommunicationException ce)
                {
                    Console.WriteLine(ce.Message);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            
            message = "The SCADA Processing Service is started.";
            Console.WriteLine("\n{0}", message);
        }

        public bool IntegrityUpdate()
        {
            return true;
                //scadaProcessing.InitiateIntegrityUpdate();
        }
    }
}
