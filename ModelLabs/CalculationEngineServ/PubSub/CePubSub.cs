using CEPubSubContract;
using FTN.Common;
using FTN.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineServ.PubSub
{
    public class CePubSub : ICEPublishContract, ICePubSubContract
    {
        public delegate void OptimizationResultEventHandler(object sender, OptimizationEventArgs e);

        public static event OptimizationResultEventHandler OptimizationResultEvent;

        private ICePubSubCallbackContract callbackCE = null;
        private OptimizationResultEventHandler optimizationResultHandler = null;

        public static string OptimizationType = "Genetic";
        // public static Action<OptimizationType> ChangeOptimizationTypeAction;

        private static List<ICePubSubCallbackContract> clientsToPublishCE = new List<ICePubSubCallbackContract>(4);

        private object clientsLockerCE = new object();

        public CePubSub()
        {
            optimizationResultHandler = new OptimizationResultEventHandler(OptimizationResultHandler);
            OptimizationResultEvent += optimizationResultHandler;
        }

        public void OptimizationResultHandler(object sender, OptimizationEventArgs e)
        {
            List<ICePubSubCallbackContract> faultetClients = new List<ICePubSubCallbackContract>(4);

            foreach (ICePubSubCallbackContract client in clientsToPublishCE)
            {
                if ((client as ICommunicationObject).State.Equals(CommunicationState.Opened))
                {
                    if (e.Message == "wind percent")
                        client.WindPercentResult(e.WindPercent);
                    else if (e.Message == "RenewableKW")
                        client.RenewableResult(e.RenewableKW);
                    else if (e.Message == "CoReduction")
                        client.PublishCoReduction(e.CoReduction);
                    else
                        client.OptimizationResults(e.OptimizationResult);
                }
                else
                {
                    faultetClients.Add(client);
                }
            }

            lock (clientsLockerCE)
            {
                foreach (ICePubSubCallbackContract client in faultetClients)
                {
                    clientsToPublishCE.Remove(client);
                }
            }
        }



        public void OptimizationResults(List<MeasurementUI> result)
        {
            OptimizationEventArgs e = new OptimizationEventArgs
            {
                OptimizationResult = result,
                Message = "Optimization result"
            };

            try
            {
                // Ovakav nacin radi na VS 2017. Prethodne verzije nemaju kompajler za C#6
                // pa ne moze da kompajlira ovakav kod
                //OptimizationResultEvent?.Invoke(this, e);
                OptimizationResultEvent(this, e);
            }
            catch (Exception ex)
            {
                string message = string.Format("CES does not have any subscribed clinet for publishing new optimization result. {0}", ex.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceVerbose, message);
                Console.WriteLine(message);
            }
        }

        public void WindPercentResult(float result)
        {
            OptimizationEventArgs e = new OptimizationEventArgs
            {
                WindPercent = result,
                Message = "wind percent"
            };
            try
            {
                OptimizationResultEvent(this, e);
            }
            catch (Exception ex)
            {
                string message = string.Format("CES does not have any subscribed clinet for publishing new optimization result. {0}", ex.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceVerbose, message);
                Console.WriteLine(message);
            }
        }

        public void RenewableResult(Tuple<DateTime, float> renewableKW)
        {
            OptimizationEventArgs e = new OptimizationEventArgs
            {
                RenewableKW = renewableKW,
                Message = "RenewableKW"
            };
            try
            {
                OptimizationResultEvent(this, e);
            }
            catch (Exception ex)
            {
                string message = string.Format("CES does not have any subscribed clinet for publishing new optimization result. {0}", ex.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceVerbose, message);
                Console.WriteLine(message);
            }
        }

        public void PublishCoReduction(Tuple<string, float, float> tupla)
        {
            OptimizationEventArgs e = new OptimizationEventArgs
            {
                CoReduction = tupla,
                Message = "CoReduction"
            };
            try
            {
                OptimizationResultEvent(this, e);
            }
            catch (Exception ex)
            {
                string message = string.Format("CES does not have any subscribed clinet for publishing new optimization result. {0}", ex.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceVerbose, message);
                Console.WriteLine(message);
            }
        }

        public void Subscribe()
        {
            callbackCE = OperationContext.Current.GetCallbackChannel<ICePubSubCallbackContract>();
            clientsToPublishCE.Add(callbackCE);
        }

        public void Unsubscribe()
        {
            callbackCE = OperationContext.Current.GetCallbackChannel<ICePubSubCallbackContract>();
            clientsToPublishCE.Remove(callbackCE);
        }
    }
}
