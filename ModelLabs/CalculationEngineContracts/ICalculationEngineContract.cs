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

    public interface ICalculationEngineContract
    {
        [OperationContract]
        bool OptimisationAlgorithm(List<MeasurementUnit> measEnergyConsumers, List<MeasurementUnit> measGenerators, float windSpeed, float sunlight);

    }
}
