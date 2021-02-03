using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using CalculationEngineContracts;
using CalculationEngineServ;
using CalculationEngineServ.PubSub;
using CEPubSubContract;
using CommonCloud;
using FTN.Common;
using FTN.ServiceContracts;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using TransactionContract;

namespace CalculationEngineCloudServ
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class CalculationEngineCloudServ : StatefulService, ICEPublishContract, ICePubSubContract
    {
        private CalculationEngine ce;
        private CeToUI ceToUI;
        private ProcessingToCalculation processingToCalculation;
        private PublisherService publisherService;

        public CalculationEngineCloudServ(StatefulServiceContext context)
            : base(context)
        {
            ce = new CalculationEngine();
            ceToUI = new CeToUI();
            processingToCalculation = new ProcessingToCalculation();
            CeToUI.Ce = ce;
            ProcessingToCalculation.CalculationEngine = ce;
            publisherService = new PublisherService();
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
            return new List<ServiceReplicaListener>
            {
                new ServiceReplicaListener(context => this.CreateCalculationEngineListener(context), "CalculationEngineEndpoint"),
                new ServiceReplicaListener(context => this.CreateCalculationEngineUIListener(context), "CalculationEngineUIEndpoint"),
                new ServiceReplicaListener(context => this.CreateCalculationEngineTransactionListener(context), "CalculationEngineTransactionEndpoint"),
                new ServiceReplicaListener(context => this.CreateCESubscribeListener(context), "CESubscribeEndpoint"),
                new ServiceReplicaListener(context => this.CreateCalculationEnginePublisherListener(context), "CEPublishEndpoint"),
                new ServiceReplicaListener(context => this.CreateCalculationEngineRepositoryListener(context), "CalculationEngineRepositoryEndpoint")
            };
        }
        private ICommunicationListener CreateCalculationEngineListener(StatefulServiceContext context)
        {
            var listener = new WcfCommunicationListener<ICalculationEngineContract>(
                           listenerBinding: Binding.CreateCustomNetTcp(),
                           endpointResourceName: "CalculationEngineEndpoint",
                           serviceContext: context,
                           wcfServiceObject: processingToCalculation
            );
            ServiceEventSource.Current.ServiceMessage(context, "Created listener for CalculationEngineEndpoint");
            return listener;
        }

        private ICommunicationListener CreateCalculationEngineRepositoryListener(StatefulServiceContext context)
        {
            var listener = new WcfCommunicationListener<ICalculationEngineRepository>(
                listenerBinding: Binding.CreateCustomNetTcp(),
                endpointResourceName: "CalculationEngineRepositoryEndpoint",
                serviceContext: context,
                wcfServiceObject: processingToCalculation
            );

            return listener;
        }

        private ICommunicationListener CreateCalculationEngineUIListener(StatefulServiceContext context)
        {
            var listener = new WcfCommunicationListener<ICalculationEngineUIContract>(
                           listenerBinding: Binding.CreateCustomNetTcp(),
                           endpointResourceName: "CalculationEngineUIEndpoint",
                           serviceContext: context,
                           wcfServiceObject: ceToUI
            );
            ServiceEventSource.Current.ServiceMessage(context, "Created listener for CalculationEngineHistoryDataEndpoint");
            return listener;
        }

        private ICommunicationListener CreateCalculationEngineTransactionListener(StatefulServiceContext context)
        {
            var listener = new WcfCommunicationListener<ITransactionContract>(
                           listenerBinding: Binding.CreateCustomNetTcp(),
                           endpointResourceName: "CalculationEngineTransactionEndpoint",
                           serviceContext: context,
                           wcfServiceObject: ce
            );
            ServiceEventSource.Current.ServiceMessage(context, "Created listener for CalculationEngineListenerEndpoint");
            return listener;
        }
        private ICommunicationListener CreateCalculationEnginePublisherListener(StatefulServiceContext context)
        {
            var listener = new WcfCommunicationListener<ICEPublishContract>(
                           listenerBinding: Binding.CreateCustomNetTcp(),
                           endpointResourceName: "CEPublishEndpoint",
                           serviceContext: context,
                           wcfServiceObject: this 
            );
            ServiceEventSource.Current.ServiceMessage(context, "Created listener for CalculationEngineHistoryDataEndpoint");
            return listener;
        }

        private ICommunicationListener CreateCESubscribeListener(StatefulServiceContext context)
        {
            var listener = new WcfCommunicationListener<ICePubSubContract>(
                           listenerBinding: Binding.CreateCustomNetTcp(),
                           endpointResourceName: "CESubscribeEndpoint",
                           serviceContext: context,
                           wcfServiceObject: this
            );

            return listener;
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            optimizationResultHandler = new OptimizationResultEventHandler(OptimizationResultHandler);
            OptimizationResultEvent += optimizationResultHandler;

            bool integrityState = ce.InitiateIntegrityUpdate();

            if (!integrityState)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, "CalculationEngine integrity update failed");
            }
            else
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, "CalculationEngine integrity update succeeded.");
            }

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }


        public delegate void OptimizationResultEventHandler(object sender, OptimizationEventArgs e);

        public static event OptimizationResultEventHandler OptimizationResultEvent;

        private ICePubSubCallbackContract callbackCE = null;
        private OptimizationResultEventHandler optimizationResultHandler = null;

        public static string OptimizationType = "Genetic";
        // public static Action<OptimizationType> ChangeOptimizationTypeAction;

        private static List<ICePubSubCallbackContract> clientsToPublishCE = new List<ICePubSubCallbackContract>(4);

        private object clientsLockerCE = new object();

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

        public bool Optimization()
        {
            throw new NotImplementedException();
        }
    }
}
