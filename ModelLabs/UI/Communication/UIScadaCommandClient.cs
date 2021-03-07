using CommonMeas;
using ScadaContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace UI.Communication
{
    public class UIScadaCommandClient : ClientBase<IScadaCommandingContract>, IScadaCommandingContract
    {
        public UIScadaCommandClient() { }
        public UIScadaCommandClient(string Endpoint) : base(Endpoint) { }
        public UIScadaCommandClient(InstanceContext callbackInstance, string endpointConfigurationName) : base(callbackInstance, endpointConfigurationName) { }

        public bool CommandAnalogValues(long gid, float value)
        {
            return Channel.CommandAnalogValues(gid, value);
        }

        public bool CommandDiscreteValues(long gid, bool value, int scadaAddress)
        {
            return Channel.CommandDiscreteValues(gid, value, scadaAddress);
        }

        public bool SendDataToSimulator(List<MeasurementUnit> measurements)
        {
            return Channel.SendDataToSimulator(measurements);
        }
    }
}
