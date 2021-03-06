using System;
using System.Collections.Generic;
using System.Fabric;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using CommonCloud;
using FTN.ServiceContracts;
using FTN.Services.NetworkModelService;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using TransactionContract;

namespace NetworkModelCloudServ
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class NetworkModelCloudServ : StatelessService
    {

        private NetworkModelCloud nm = null;
        private GenericDataAccessCloud gda = null;

        public NetworkModelCloudServ(StatelessServiceContext context)
            : base(context)
        {
            gda = new GenericDataAccessCloud();
            nm = new NetworkModelCloud();
            GenericDataAccessCloud.NetworkModel = nm;
            ResourceIteratorCloud.NetworkModel = nm;
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new List<ServiceInstanceListener>
            {
                new ServiceInstanceListener(context=>this.CreateNetworkModelGDAListener(context), "NetworkModelGDAEndpoint"),
                //new ServiceInstanceListener(context=>this.CreateNetworkModelGDAListener1(context), "NetworkModelGDAEndpoint"),
                new ServiceInstanceListener(context=>this.CreateNMSTransactionListener(context), "NMSTranscationEndpoint")
            };
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            //long iterations = 0;
            ServiceEventSource.Current.ServiceMessage(this.Context, "NetworkModelCloudService");

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                //ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        private ICommunicationListener CreateNetworkModelGDAListener(StatelessServiceContext context)
        {
            var listener = new WcfCommunicationListener<INetworkModelGDAContract>(
                listenerBinding: Binding.CreateCustomNetTcp(),
                //address : new EndpointAddress("net.tcp://localhost:10000/NetworkModelCloudServ/GDA/"), 
                endpointResourceName: "NetworkModelGDAEndpoint",
                serviceContext: context,
                wcfServiceObject: gda
            );

            return listener;
        }

        private ICommunicationListener CreateNMSTransactionListener(StatelessServiceContext context)
        {
            var listener = new WcfCommunicationListener<ITransactionContract>(
                listenerBinding: Binding.CreateCustomNetTcp(),
                endpointResourceName: "NMSTranscationEndpoint",
                serviceContext: context,
                wcfServiceObject: nm
            );

            return listener;

        }

        //private ICommunicationListener CreateNetworkModelGDAListener1(StatelessServiceContext context)
        //{
        //    //string host = context.NodeContext.IPAddressOrFQDN;

        //    //var endpointConfig = context.CodePackageActivationContext.GetEndpoint("NetworkModelGDAEndpoint");
        //    //int port = endpointConfig.Port;
        //    //var scheme = endpointConfig.UriScheme.ToString();
        //    //var pathSufix = endpointConfig.PathSuffix.ToString();

        //    //string uri = string.Format(CultureInfo.InvariantCulture, "{0}://{1}:{2}/NetworkModelService/{3}", scheme, host, port, pathSufix);

        //    var listener = new WcfCommunicationListener<INetworkModelGDAContract>(
        //                    listenerBinding: Binding.CreateCustomNetTcp(),
        //                    address: new EndpointAddress("net.tcp://localhost:10000/NetworkModelService/GDA/"),
        //                    serviceContext: context,
        //                    wcfServiceObject: new NetworkModelGDA()
        //    );

        //    return listener;
        //}
    }
}
