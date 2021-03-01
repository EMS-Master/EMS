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
    public interface ICo2EmissionAndReduction
    {
        [OperationContract]
        float CalculateCO2ReductionWithBiggestCoeficient(Dictionary<long, OptimisationModel> optModelMap);
        [OperationContract]
        float CalculateCO2(Dictionary<long, OptimisationModel> optModelMap);
        [OperationContract]
        float CalculateCO2WithKyotoProtocol(Dictionary<long, OptimisationModel> optModelMap);

    }
}
