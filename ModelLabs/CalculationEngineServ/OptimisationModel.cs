using CommonMeas;
using FTN.Common;
using FTN.Services.NetworkModelService.DataModel.Wires;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineServ
{
    public class OptimisationModel
    {
        public long GlobalId { get; set; }
        public float Price { get; set; }
        public float MeasuredValue { get; set; }
        public float GenericOptimizedValue { get; set; }
        public float MinPower { get; set; }
        public float MaxPower { get; set; }
        public bool Renewable { get; set; }
        public GeneratorType TypeGenerator { get; set; }
        public OptimisationModel(Generator g, MeasurementUnit mu, float windSpeed, float sunlight)
        {
            GlobalId = g.GlobalId;
            MeasuredValue = mu.CurrentValue;
            TypeGenerator = g.GeneratorType;
            GenericOptimizedValue = 0; // izracunati			

            Renewable = (g.GeneratorType.Equals(GeneratorType.Wind) || g.GeneratorType.Equals(GeneratorType.Sollar)) ? true : false;

            Price = 0;
            MaxPower = g.MaxQ;
        }
        public float CalculatePrice(float measuredValue)
        {
            if (Renewable)
            {
                return 1;
            }
            float price=0;

            if (TypeGenerator.Equals(GeneratorType.Coal))
            {
                price = 10;
            }
            else if (TypeGenerator.Equals(GeneratorType.Coal))
            {
                price = 20;
            }
            else if (TypeGenerator.Equals(GeneratorType.Coal))
            {
                price = 30;
            }            
            return price;
        }

    }
}
