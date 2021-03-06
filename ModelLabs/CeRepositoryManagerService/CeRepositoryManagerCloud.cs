using CalculationEngineContracts;
using CalculationEngineServ;
using CommonCloud.AzureStorage.Entities;
using CommonCloud.AzureStorage.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CeRepositoryManagerService
{
    public class CeRepositoryManagerCloud : ICeRepositoryManager
    {
        public CeRepositoryManagerCloud() { }
        public List<long> GetGidsForCommandedGenerators()
        {
            return DbManager.Instance.GetCommandedGenerators().Where(x => x.CommandingFlag).Select(y => y.Gid).ToList();
        }

        public void AddHistoryMeasurement(HistoryMeasurementHelper historyMeasurement)
        {
            HistoryMeasurement historyMeasurementDb = new HistoryMeasurement(historyMeasurement);
            DbManager.Instance.AddHistoryMeasurement(historyMeasurementDb);
        }

        public List<HistoryMeasurementHelper> GetAllHistoryMeasurementsForSelectedTime(DateTime startTime, DateTime endTime, long gid)
        {
            var listOfHistoryMeasurements = DbManager.Instance.GetAllHistoryMeasurementsForSelectedTime(startTime, endTime, gid);
            return listOfHistoryMeasurements.Select(x => new HistoryMeasurementHelper()
            {
                Gid = x.Gid,
                Id = x.Id,
                MeasurementTime = x.MeasurementTime,
                MeasurementValue = x.MeasurementValue
            }).ToList();
        }

        public void AddTotalProduction(TotalProductionHelper totalProduction)
        {
            TotalProduction totalProductionDb = new TotalProduction(totalProduction);
            DbManager.Instance.AddTotalProduction(totalProductionDb);

            string message = string.Format("Total production added in DB: \n");
            message += string.Format("Total generation: {0}\n", totalProduction.TotalGeneration);
            message += string.Format("Total CO2 reduction: {0}\n", totalProduction.CO2Reduction);
            message += string.Format("Total CO2 emission: {0}\n", totalProduction.CO2Emission);
            message += string.Format("Total cost: {0}\n", totalProduction.TotalCost);
            message += string.Format("Total profit: {0}\n", totalProduction.Profit);
            message += string.Format("Time of calculation: {0}\n", totalProduction.TimeOfCalculation);

            ServiceEventSource.Current.Message(message);
        }

        public List<TotalProductionHelper> GetTotalProductionsForSelectedTime(DateTime startTime, DateTime endTime)
        {
            string message = string.Format("Method GetTotalProductionsForSelectedTime():\n");
            message += string.Format("start time: {0}\n", startTime);
            message += string.Format("end time: {0}\n", endTime);

            ServiceEventSource.Current.Message(message);

            var listOfTotalProductions = DbManager.Instance.GetTotalProductionsForSelectedTime(startTime, endTime);
            return listOfTotalProductions.Select(x => new TotalProductionHelper(x)).ToList();
        }

        public List<CommandedGeneratorHelper> GetCommandedGenerators()
        {
            var listOfCommandedGenerators = DbManager.Instance.GetCommandedGenerators();
            return listOfCommandedGenerators.Select(x => new CommandedGeneratorHelper(x)).ToList();
        }

        public void AddListCommandedGenerators(List<CommandedGeneratorHelper> commandedGenerators)
        {
            var commandedGeneratosDb = commandedGenerators.Select(x => new CommandedGenerator(x)).ToList();
            DbManager.Instance.AddListCommandedGenerators(commandedGeneratosDb);
        }

        public CommandedGeneratorHelper GetCommandedGenerator(long gid)
        {
            return new CommandedGeneratorHelper(DbManager.Instance.GetCommandedGenerator(gid));
        }

        public void AddCommandedGenerator(CommandedGeneratorHelper commandedGenerator)
        {
            DbManager.Instance.AddCommandedGenerator(new CommandedGenerator(commandedGenerator));
        }
    }
}
