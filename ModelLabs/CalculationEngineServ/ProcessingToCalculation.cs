using CalculationEngineContracts;
using CommonMeas;
using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineServ
{
    public class ProcessingToCalculation : ICalculationEngineContract
    {
        private static CalculationEngine ce = null;

        public ProcessingToCalculation()
        {
        }

        public static CalculationEngine CalculationEngine { get => ce; set => ce = value; }

        public bool OptimisationAlgorithm(List<MeasurementUnit> measEnergyConsumers, List<MeasurementUnit> measGenerators, float windSpeed, float sunlight)
        {
            bool retVal = false;
            try
            {
                retVal = CalculationEngine.Optimize(measEnergyConsumers, measGenerators, windSpeed, sunlight);
            }
            catch (Exception ex)
            {
                string message = string.Format("Error: {0}", ex.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, message);
            }

            return retVal;
        }
    }
}
