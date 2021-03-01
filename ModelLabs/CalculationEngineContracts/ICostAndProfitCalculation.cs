using CommonMeas;
using FTN.Common;
using FTN.Services.NetworkModelService.DataModel.Wires;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineContracts
{
    [ServiceContract]
    public interface ICostAndProfitCalculation
    {
        [OperationContract]
        float CalculateTotalCostWhenNecessaryEnergyIsZero(Dictionary<long, OptimisationModel> optModelMap, float TotalCost);
        [OperationContract]
        float CalculateProfit(Dictionary<long, OptimisationModel> allGenerators,
            IDictionary<long, Generator> generators,
            Dictionary<GeneratorType, float> allTypes,
            List<GeneratorCurveModel> generatorCurves);
    }
}
