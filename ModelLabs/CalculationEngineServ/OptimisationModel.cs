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
		public float EmissionFactor { get; set; }
		public MeasurementUnit measurementUnit { get; set; }
        public OptimisationModel(Generator g, MeasurementUnit mu, float windSpeed, float sunlight)
        {
            GlobalId = g.GlobalId;
            MeasuredValue = mu.CurrentValue;
            Fuel = new Tuple<GeneratorType, float>(g.GeneratorType, GetUnitPrice(g.GeneratorType)); //gorivo i cena po jedinici kolicine
            GenericOptimizedValue = 0;
            TypeGenerator = g.GeneratorType;
            Renewable = (g.GeneratorType.Equals(GeneratorType.Wind) || g.GeneratorType.Equals(GeneratorType.Solar) || g.GeneratorType.Equals(GeneratorType.Hydro)) ? true : false;
			measurementUnit = mu;

            Price = 0;
            MaxPower = g.MaxQ;
			MinPower = g.MinQ;
			EmissionFactor = SetEmissionFactor(g.GeneratorType);
        }
        public float CalculatePrice(float energy)
        {
            if (Renewable)
            {
                return 0;
            }
            float price = 0;
			float fuelQuantity = energy / 1000f;
			price = Fuel.Item2 * fuelQuantity;
		
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

		public float SetEmissionFactor(GeneratorType generatorType)
		{
			switch (generatorType)
			{
				case GeneratorType.Coal:
					return 0.30f;
				case GeneratorType.Oil:
					return 0.25f;
				case GeneratorType.Gas:
					return 0.25f;
				default:		//Wind, Solar, Hydro
					return 0f;
			}
		}
	}
}
