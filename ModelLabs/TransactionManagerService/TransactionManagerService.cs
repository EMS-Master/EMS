using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace TransactionManagerService
{
    public class TransactionManagerService : IDisposable
    {
        private TransactionManager transactionManager = null;
        private List<ServiceHost> hosts = null;

        public TransactionManagerService()
        {
            transactionManager = new TransactionManager();
            InitializeHosts();
        }

        public void Start()
        {
            StartHosts();
        }

        private void InitializeHosts()
        {
            hosts = new List<ServiceHost>();
            hosts.Add(new ServiceHost(typeof(TransactionManager)));
        }

        private void StartHosts()
        {
            if (hosts == null || hosts.Count == 0)
            {
                throw new Exception("Transaction Manager Services can not be opend because it is not initialized.");
            }

            string message = string.Empty;
            foreach (ServiceHost host in hosts)
            {
                try
                {
                    host.Open();

                    message = string.Format("The WCF service {0} is ready.", host.Description.Name);
                    Console.WriteLine(message);
                    CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);

                    foreach (Uri uri in host.BaseAddresses)
                    {
                        Console.WriteLine(uri);
                        CommonTrace.WriteTrace(CommonTrace.TraceInfo, uri.ToString());
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

            message = string.Format("Trace level: {0}", CommonTrace.TraceLevel);
            Console.WriteLine(message);
            CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);

            message = "The Transaction Manager Service is started.";
            Console.WriteLine("\n{0}", message);
            CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
        }

        public void Dispose()
        {
            CloseHosts();
            GC.SuppressFinalize(this);
        }

        public void CloseHosts()
        {
            if (hosts == null || hosts.Count == 0)
            {
                throw new Exception("Transaction Manager Services can not be closed because it is not initialized.");
            }

            foreach (ServiceHost host in hosts)
            {
                host.Close();
            }

            string message = "The Transaction Managers Service is closed.";
            CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
            Console.WriteLine("\n\n{0}", message);
        }

        public void ScadaCRPrepare(ref Delta analogDelta)
        {
            //TransactionScadaPRProxy.Instance.Prepare(ref analogDelta);
        }

        public void ScadaCMDPrepare(Delta analogDelta)
        {
           // TransactionScadaCDProxy.Instance.Prepare(ref analogDelta);
        }

        public void NMSPrepare(Delta delta)
        {
           // TransactionNMSProxy.Instance.Prepare(ref delta);
        }

    }
}
