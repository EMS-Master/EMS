using CommonMeas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineContracts
{
    [ServiceContract]
    public interface ICalculationEngineRepository
    {
        [OperationContract]
        List<DiscreteCounterModel> GetAllDiscreteCounters();

        [OperationContract]
        void InsertOrUpdate(DiscreteCounterModel model);

    }
}
