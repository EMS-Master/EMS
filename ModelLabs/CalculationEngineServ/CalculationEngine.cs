using CalculationEngineServ.GeneticAlgorithm;
using CommonMeas;
using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineServ
{
    public class CalculationEngine 
    {
        public bool Optimize(List<MeasurementUnit> measEnergyConsumers, List<MeasurementUnit> measGenerators, float windSpeed, float sunlight)
        {
            bool result = false;
            Console.WriteLine("wind speed: {0}, sun light: {1}.", windSpeed, sunlight);
            foreach(var m in measGenerators)
            {
                Console.WriteLine("masx value: " + m.MaxValue);
            }
            GA ga = new GA();
            return result;
        }
    }
}
