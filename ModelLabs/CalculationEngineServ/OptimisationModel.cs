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
        public float MeasuredValue { get; set; } // values collected from scada
        public float GenericOptimizedValue { get; set; }
        public float MinPower { get; set; }
        public float MaxPower { get; set; }
        public bool Renewable { get; set; }
        public Tuple<GeneratorType, float> Fuel { get; set; } // gorivo i cena po jedinici
        public GeneratorType TypeGenerator { get; set; }
        public OptimisationModel(Generator g, MeasurementUnit mu, float windSpeed, float sunlight)
        {
            GlobalId = g.GlobalId;
            MeasuredValue = mu.CurrentValue;
            Fuel = new Tuple<GeneratorType, float>(g.GeneratorType, GetUnitPrice(g.GeneratorType)); //gorivo i cena po jedinici kolicine
            GenericOptimizedValue = 0;
            TypeGenerator = g.GeneratorType;
            Renewable = (g.GeneratorType.Equals(GeneratorType.Wind) || g.GeneratorType.Equals(GeneratorType.Sollar) || g.GeneratorType.Equals(GeneratorType.Hydro)) ? true : false;

            Price = 0;
            MaxPower = g.MaxQ;
        }
        public float CalculatePrice(float energy)
        {
            if (Renewable)
            {
                return 1;
            }
            float price = 0;

            if (TypeGenerator.Equals(GeneratorType.Coal))
            {
                price = energy * 10;
            }
            else if (TypeGenerator.Equals(GeneratorType.Gas))
            {
                price = energy * 20;
            }
            else if (TypeGenerator.Equals(GeneratorType.Oil))
            {
                price = energy * 30;
            }
            return price;
        }
        public float GetUnitPrice(GeneratorType gType)
        {
            if (gType.Equals(GeneratorType.Coal))
            {
                return 1f;
            }
            else if (gType.Equals(GeneratorType.Gas))
            {
                return 2f;
            }
            else if (gType.Equals(GeneratorType.Oil))
            {
                return 3f;
            }
            else
            {
                return 0f;
            }
        }

    }
}
