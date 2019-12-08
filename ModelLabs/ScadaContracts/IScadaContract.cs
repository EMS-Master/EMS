using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ScadaContracts
{
    [ServiceContract]
    public interface IScadaContract
    {
        [OperationContract]
        bool GetDataFromSimulator();
    }
}
