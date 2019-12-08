using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ScadaService
{
    public class ScadaService : IDisposable
    {
        private Scada scada = null;
        private List<ServiceHost> hosts = null;

        public ScadaService()
        {
            this.scada = new Scada();
            this.InitializeHosts();
        }

        public void Start()
        {
            this.StartHosts();
        }

        public void StartCollectingData()
        {
            this.scada.StartCollectingData();
        }
        public void Dispose()
        {
            this.CloseHosts();
            GC.SuppressFinalize(this);
        }

        private void InitializeHosts()
        {
            this.hosts = new List<ServiceHost>();
            this.hosts.Add(new ServiceHost(typeof(Scada)));

        }

        private void StartHosts()
        {
            if(this.hosts == null || this.hosts.Count == 0)
            {
                throw new Exception("Scada service cannot be opened because it is not initialized.");
            }
            String message = String.Empty;
            foreach(ServiceHost host in this.hosts)
            {
                try
                {
                    host.Open();
                    Console.WriteLine("The WCF Service {0} is ready.", host.Description.Name);


                }
                catch(CommunicationException ce)
                {
                    Console.WriteLine(ce.Message);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            Console.WriteLine("The Scada service is started.");
        }

        private void CloseHosts()
        {
            if (this.hosts == null || this.hosts.Count == 0)
            {
                throw new Exception("Scada service cannot be closed because it is not initialized.");
            }
            foreach(ServiceHost host in this.hosts)
            {
                host.Close();
            }
            Console.WriteLine("The Scada service is closed");
        }

    }
}
