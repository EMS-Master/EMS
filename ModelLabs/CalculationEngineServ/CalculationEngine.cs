using CalculationEngineServ.GeneticAlgorithm;
using CommonMeas;
using FTN.Common;
using ScadaContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionContract;

namespace CalculationEngineServ
{
    public class CalculationEngine : ITransactionContract
    {
        public bool Commit()
        {
            throw new NotImplementedException();
        }

        public bool Optimize(List<MeasurementUnit> measEnergyConsumers, List<MeasurementUnit> measGenerators, float windSpeed, float sunlight)
        {
            bool result = false;
            Console.WriteLine("wind speed: {0}, sun light: {1}.", windSpeed, sunlight);
            foreach(var m in measGenerators)
            {
                Console.WriteLine("masx value: " + m.MaxValue);
            }
            GA ga = new GA();

            //todo optimization and put into measuremnetOptimized list
            //for now, lets put parameter measGenerators
            List<MeasurementUnit> measurementsOptimized = measGenerators;

            try
            {
                if (ScadaCommandingProxy.Instance.SendDataToSimulator(measurementsOptimized))
                {
                    CommonTrace.WriteTrace(CommonTrace.TraceInfo, "CE sent {0} optimized MeasurementUnit(s) to SCADACommanding.", measurementsOptimized.Count);
                    Console.WriteLine("CE sent {0} optimized MeasurementUnit(s) to SCADACommanding.", measurementsOptimized.Count);

                    result = true;
                }
            }
            catch (Exception ex)
            {
                CommonTrace.WriteTrace(CommonTrace.TraceError, ex.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, ex.StackTrace);
            }
            return result;
        }

        public UpdateResult Prepare(ref Delta delta)
        {
            throw new NotImplementedException();
        }

        public bool Rollback()
        {
            throw new NotImplementedException();
        }
    }
}
