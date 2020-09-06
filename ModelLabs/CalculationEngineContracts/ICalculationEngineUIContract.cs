using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineContracts
{
    [ServiceContract]
    public interface ICalculationEngineUIContract
    {
        [OperationContract]
        List<Tuple<double, DateTime>> GetHistoryMeasurements(long gid, DateTime startTime, DateTime endTime);

        [OperationContract]
        List<Tuple<double, DateTime>> GetTotalProduction(DateTime startTime, DateTime endTime);

        [OperationContract]
        bool SetAlgorithmOptions(int iterationCount, int populationCount, int elitisamPct, float mutationRate);

        [OperationContract]
        bool SetAlgorithmOptionsDefault();
    }
}
