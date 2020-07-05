using CommonMeas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ScadaContracts
{
    [ServiceContract]
    public interface IScadaCommandingContract
    {
        [OperationContract]
        bool SendDataToSimulator(List<MeasurementUnit> measurements);
        [OperationContract]
        bool CommandDiscreteValues(List<MeasurementUnitDiscrete> measurements);
    }
}
