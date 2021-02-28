using CommonCloud.AzureStorage.Entities;
using CommonCloud.AzureStorage.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineContracts
{
    [ServiceContract]
    public interface ICeRepositoryManager
    {
        [OperationContract]
        List<long> GetGidsForCommandedGenerators();
        [OperationContract]
        void AddHistoryMeasurement(HistoryMeasurementHelper historyMeasurement);
        [OperationContract]
        List<HistoryMeasurementHelper> GetAllHistoryMeasurementsForSelectedTime(DateTime startTime, DateTime endTime, long gid);
        [OperationContract]
        void AddTotalProduction(TotalProductionHelper totalProduction);
        [OperationContract]
        List<TotalProductionHelper> GetTotalProductionsForSelectedTime(DateTime startTime, DateTime endTime);
        [OperationContract]
        List<CommandedGeneratorHelper> GetCommandedGenerators();
        [OperationContract]
        void AddListCommandedGenerators(List<CommandedGeneratorHelper> commandedGenerators);
        [OperationContract]
        CommandedGeneratorHelper GetCommandedGenerator(long gid);
        [OperationContract]
        void AddCommandedGenerator(CommandedGeneratorHelper commandedGenerator);
    }
}
