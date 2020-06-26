using FTN.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using FTN.Common;

namespace CalculationEngineServ.PubSub
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class PublisherService : ICePubSubContract
    {
        public delegate void OptimizationResultEventHandler(object sender, OptimizationEventArgs e);
        public static event OptimizationResultEventHandler OptimizationResultEvent;

        private ICePubSubCallbackContract callback = null;
        private OptimizationResultEventHandler optimizationResultHandler = null;

        private static List<ICePubSubCallbackContract> clientsToPublish = new List<ICePubSubCallbackContract>(4);

        private object clientsLocker = new object(); 

        public PublisherService()
        {
            optimizationResultHandler = new OptimizationResultEventHandler(OptimizationResultHandler);
            OptimizationResultEvent += optimizationResultHandler;
        }

        public void OptimizationResultHandler(object sender, OptimizationEventArgs e)
        {
            List<ICePubSubCallbackContract> faultetClients = new List<ICePubSubCallbackContract>(4);

            foreach(ICePubSubCallbackContract client in clientsToPublish)
            {
                if((client as ICommunicationObject).State.Equals(CommunicationState.Opened))
                {
                    client.OptimizationResults(e.OptimizationResult);
                }
                else
                {
                    faultetClients.Add(client);
                }
            }

            lock (clientsLocker)
            {
                foreach(ICePubSubCallbackContract client in faultetClients)
                {
                    clientsToPublish.Remove(client);
                }
            }
        }
            public void Subscribe()
        {
            callback = OperationContext.Current.GetCallbackChannel<ICePubSubCallbackContract>();
            clientsToPublish.Add(callback);
        }

        public void Unsubscribe()
        {
            callback = OperationContext.Current.GetCallbackChannel<ICePubSubCallbackContract>();
            clientsToPublish.Remove(callback);
        }

        public void PublishOptimizationResults(List<MeasurementUI> result)
        {
            OptimizationEventArgs e = new OptimizationEventArgs
            {
                OptimizationResult = result,
                Message="optimization result"
            };
            try
            {
                OptimizationResultEvent(this, e);
            }
            catch(Exception ex)
            {
                string message = string.Format("CES does not have any subscribed clinet for publishing new optimization result. {0}", ex.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceVerbose, message);
                Console.WriteLine(message);
            }
        }
        
        
    }
}
