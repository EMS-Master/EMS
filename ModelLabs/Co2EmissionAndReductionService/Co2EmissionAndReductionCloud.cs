using CalculationEngineContracts;
using CommonMeas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Co2EmissionAndReductionService
{
    public class Co2EmissionAndReductionCloud : ICo2EmissionAndReduction
    {
        public Co2EmissionAndReductionCloud() { }

        public float CalculateCO2(Dictionary<long, OptimisationModel> optModelMap)
        {
            //CO2 Emissions from each fuel (tonnes) = Energy consumption of fuel (kWh) x Emission factor for each fuel (kgCO2/kWh) x 0.001
            float emCO2 = 0;
            foreach (var optModel in optModelMap.Values)
            {
                emCO2 += optModel.GenericOptimizedValue * optModel.EmissionFactor * 0.001f;
            }

            string message = string.Format("Calculated emission CO2: {0}", emCO2);
            //ServiceEventSource.Current.Message(message);
            return emCO2;
        }

        public float CalculateCO2ReductionWithBiggestCoeficient(Dictionary<long, OptimisationModel> optModelMap)
        {
            //CO2 Emissions from each fuel (tonnes) = Energy consumption of fuel (kWh) x Emission factor for each fuel (kgCO2/kWh) x 0.001
            float reductionCO2 = optModelMap.Values.Sum(x => (x.GenericOptimizedValue)) * 0.30f * 0.001f;

            string message = string.Format("Calculated reduction CO2: {0}", reductionCO2);
            //ServiceEventSource.Current.Message(message);
            return reductionCO2;
        }

        public float CalculateCO2WithKyotoProtocol(Dictionary<long, OptimisationModel> optModelMap)
        {
            float kyotoCoefficient = 0.008f; //	EU - 0.8%
            var co2WithKyoto = optModelMap.Values.Sum(x => (x.GenericOptimizedValue / 1000f) * kyotoCoefficient);

            string message = string.Format("Calculated CO2 with Kyoto: {0}", co2WithKyoto);
            //ServiceEventSource.Current.Message(message);
            return co2WithKyoto;
        }
    }
}
