using CalculationEngineContracts;
using CommonMeas;
using FTN.Common;
using FTN.Services.NetworkModelService.DataModel.Wires;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CostAndProfitClaculationService
{
    public class CostAndProfitCalculationCloud : ICostAndProfitCalculation
    {
        public CostAndProfitCalculationCloud() { }

        public float CalculateProfit(
            Dictionary<long, OptimisationModel> allGenerators,
            IDictionary<long, Generator> generators,
            Dictionary<GeneratorType, float> allTypes,
            List<GeneratorCurveModel> generatorCurves)
        {
            float profitValue = 0;
            Dictionary<GeneratorType, float> maxPowerPerFuel = new Dictionary<GeneratorType, float>();

            float sumOfRenewables = allGenerators.Where(x => x.Value.Renewable).Select(u => u.Value.measurementUnit.CurrentValue).Sum();

            maxPowerPerFuel.Add(GeneratorType.Oil, generators.Where(x => x.Value.GeneratorType == GeneratorType.Oil).Sum(y => y.Value.MaxQ));
            maxPowerPerFuel.Add(GeneratorType.Coal, generators.Where(x => x.Value.GeneratorType == GeneratorType.Coal).Sum(y => y.Value.MaxQ));
            maxPowerPerFuel.Add(GeneratorType.Gas, generators.Where(x => x.Value.GeneratorType == GeneratorType.Gas).Sum(y => y.Value.MaxQ));

            maxPowerPerFuel[GeneratorType.Oil] -= allGenerators.Where(x => x.Value.TypeGenerator == GeneratorType.Oil).Sum(y => y.Value.MeasuredValue);
            maxPowerPerFuel[GeneratorType.Coal] -= allGenerators.Where(x => x.Value.TypeGenerator == GeneratorType.Coal).Sum(y => y.Value.MeasuredValue);
            maxPowerPerFuel[GeneratorType.Gas] -= allGenerators.Where(x => x.Value.TypeGenerator == GeneratorType.Gas).Sum(y => y.Value.MeasuredValue);

            foreach (var item in allTypes)
            {
                if (maxPowerPerFuel[item.Key] >= sumOfRenewables)
                {
                    //profitValue += (item.Value * (sumOfRenewables/1000f));
                    float percentage = (100 * (sumOfRenewables / 1000f)) / (maxPowerPerFuel[item.Key] / 1000f);
                    GeneratorCurveModel genCurveModel = generatorCurves.FirstOrDefault(x => x.LowerPoint <= percentage && x.HigherPoint >= percentage && x.GeneratorType.Contains(item.Key.ToString()));
                    float fuelQuantityPerMW = (float)genCurveModel.A * percentage + (float)genCurveModel.B;       //[t/MW]
                    float fuelQuantity = fuelQuantityPerMW * sumOfRenewables / 1000f;
                    profitValue += item.Value * fuelQuantity;
                    break;
                }
                else
                {
                    sumOfRenewables -= maxPowerPerFuel[item.Key];
                    float percentage = (100 * (sumOfRenewables / 1000f)) / (maxPowerPerFuel[item.Key] / 1000f);
                    percentage = percentage / 100f;
                    GeneratorCurveModel genCurveModel = generatorCurves.FirstOrDefault(x => x.LowerPoint <= percentage && x.HigherPoint >= percentage && x.GeneratorType.Contains(item.Key.ToString()));
                    float fuelQuantityPerMW = (float)genCurveModel.A * percentage + (float)genCurveModel.B;       //[t/MW]
                    float fuelQuantity = fuelQuantityPerMW * sumOfRenewables / 1000f;
                    profitValue += item.Value * fuelQuantity;
                    //profitValue += (item.Value * (sumOfRenewables / 1000f));
                }
            }

            string message = string.Format("Calculated profit: {0}", profitValue);
            //ServiceEventSource.Current.Message(message);
            return profitValue;
        }

        public float CalculateTotalCostWhenNecessaryEnergyIsZero(Dictionary<long, OptimisationModel> optModelMap, float totalCost)
        {
            foreach (var item in optModelMap)
            {
                float price = item.Value.CalculatePrice(item.Value.GenericOptimizedValue);
                item.Value.Price = price;
                totalCost += price;
            }

            string message = string.Format("Calculated cost: {0}", totalCost);
            //ServiceEventSource.Current.Message(message);

            return totalCost;
        }
    }
}
