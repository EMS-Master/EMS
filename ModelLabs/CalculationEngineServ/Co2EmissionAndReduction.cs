using CalculationEngineContracts;
using CommonMeas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineServ
{
    public class Co2EmissionAndReduction : ICo2EmissionAndReduction
    {
        public Co2EmissionAndReduction() { }

        public float CalculateCO2(Dictionary<long, OptimisationModel> optModelMap)
        {
            //CO2 Emissions from each fuel (tonnes) = Energy consumption of fuel (kWh) x Emission factor for each fuel (kgCO2/kWh) x 0.001
            float emCO2 = 0;
            foreach (var optModel in optModelMap.Values)
            {
                emCO2 += optModel.GenericOptimizedValue * optModel.EmissionFactor * 0.001f;
            }
            return emCO2;
        }

        public float CalculateCO2ReductionWithBiggestCoeficient(Dictionary<long, OptimisationModel> optModelMap)
        {
            //CO2 Emissions from each fuel (tonnes) = Energy consumption of fuel (kWh) x Emission factor for each fuel (kgCO2/kWh) x 0.001
            float reductionCO2 = optModelMap.Values.Sum(x => (x.GenericOptimizedValue)) * 0.30f * 0.001f;
            return reductionCO2;

        }

        public float CalculateCO2WithKyotoProtocol(Dictionary<long, OptimisationModel> optModelMap)
        {
            float kyotoCoefficient = 0.008f; //	EU - 0.8%
            return optModelMap.Values.Sum(x => (x.GenericOptimizedValue / 1000f) * kyotoCoefficient);

        }
    }
}
