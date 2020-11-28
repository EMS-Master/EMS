using CalculationEngineServ.DataBaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineServ.GeneticAlgorithm
{
    public class GA
    {
        private  int ELITIMS_PERCENTAGE;
        private  int NUMBER_OF_ITERATION ;
        private  int NUMBER_OF_POPULATION;
        private  float mutationRate;
        private Random random;
        private GeneticAlgorithm<Tuple<long, float>> ga;
        private Dictionary<long, OptimisationModel> optModelMap;
        private Dictionary<int, long> indexToGid;
        public float NecessaryEnergy { get; set; }
        public float TotalCost { get; private set; }
        public float GeneratedPower { get; private set; }
        public float EmissionCO2 { get; private set; }
		private Dictionary<long, OptimisationModel> commandedGeneratrs;
		public Dictionary<long, float> CommandedGenGidsAndValues { get; set;}
		public float PointX;
		public float PointY;
        public GA(float necessaryEnergy, Dictionary<long, OptimisationModel> optModelMap)
        {
			EmsContext e = new EmsContext();
			commandedGeneratrs = new Dictionary<long, OptimisationModel>();
			CommandedGenGidsAndValues = e.CommandedGenerators.Where(x => x.CommandingFlag).ToDictionary(x => x.Gid,x=> x.CommandingValue);
			this.optModelMap = optModelMap.Where(x => !CommandedGenGidsAndValues.Any(y => y.Key == x.Key)).ToDictionary(param => param.Key, param => param.Value);
			
			foreach(var item in CommandedGenGidsAndValues)
			{
				var comm = optModelMap.FirstOrDefault(x => x.Key == item.Key);
                if(comm.Value != null)
                {
                    comm.Value.MeasuredValue = item.Value;
                    comm.Value.measurementUnit.CurrentValue = item.Value;
                    comm.Value.GenericOptimizedValue = item.Value;
                    commandedGeneratrs.Add(item.Key, comm.Value);
                }
				
			}

			indexToGid = new Dictionary<int, long>();
            int i = 0;
            foreach (var valPair in this.optModelMap)
            {
                indexToGid.Add(i++, valPair.Key);
            }

			this.NecessaryEnergy = necessaryEnergy - commandedGeneratrs.Sum(x => x.Value.MeasuredValue);
			if (this.NecessaryEnergy <= 0)
				this.NecessaryEnergy = 0;
		}

        private Tuple<long, float> GetRandomGene(int index)
        {
            long gid = indexToGid[index];
			if(optModelMap[gid] != null)
			{
				var minPower = optModelMap[gid].MinPower;
				var maxPower = optModelMap[gid].MaxPower;
				float randNumb = (float)GetRandomNumber(minPower, maxPower);
				return new Tuple<long, float>(gid, randNumb);
			}
			return null;
            
        }
        private double GetRandomNumber(float minPower, float maxPower)
        {
            return random.NextDouble() * (maxPower - minPower);
        }
        private float FitnessFunction(DNA<Tuple<long, float>> dna)
        {
            return CalculateCost(dna.Genes);
        }
        private Tuple<long, float> MutateFunction(Tuple<long, float> gene, float mutateRate)
        {
            long gid = gene.Item1;

            double rndNumber = random.NextDouble();
            float mutateOffset = 0;

            if (rndNumber < 0.5)
            {
                mutateOffset = -mutateRate;
            }
            else if (rndNumber > 0.5)
            {
                mutateOffset = +mutateRate;
            }

            float mutatedGeneValue = gene.Item2 + mutateOffset;

            if (mutatedGeneValue < optModelMap[gid].MinPower)
            {
                mutatedGeneValue = optModelMap[gid].MinPower;
            }
            else if (mutatedGeneValue > optModelMap[gid].MaxPower)
            {
                mutatedGeneValue = optModelMap[gid].MaxPower;
            }
            return new Tuple<long, float>(gid, mutatedGeneValue);
        }
        public Dictionary<long, OptimisationModel> StartAlgorithm(int iterationsCount, int populationNumber, int elitisam, float mutationRat)
        {
            SetAlgorithmOptions(iterationsCount, populationNumber, elitisam, mutationRat);
            random = new Random();
            ga = new GeneticAlgorithm<Tuple<long, float>>(NUMBER_OF_POPULATION, optModelMap.Count, random, GetRandomGene,
														FitnessFunction, MutateFunction, ELITIMS_PERCENTAGE, NecessaryEnergy,
														ScaleDNA, mutationRate, false);

            ga.Population = PopulateFirstPopulation();
            Tuple<long, float>[] bestGenes = ga.StartAndReturnBest(NUMBER_OF_ITERATION);

            foreach (var item in bestGenes)
            {
				optModelMap.FirstOrDefault(x => x.Key == item.Item1).Value.GenericOptimizedValue = item.Item2;
				optModelMap.FirstOrDefault(x => x.Key == item.Item1).Value.MeasuredValue = item.Item2;
				optModelMap.FirstOrDefault(x => x.Key == item.Item1).Value.measurementUnit.CurrentValue = item.Item2;
			}

			foreach(var item in commandedGeneratrs)
			{
				optModelMap.Add(item.Key, item.Value);
			}
			

            float emCO2 = CalculationEngine.CalculateCO2(optModelMap);

            EmissionCO2 = emCO2;

            TotalCost = CalculateCost();

            Console.WriteLine("Necessery energy: " + NecessaryEnergy);

            return optModelMap;
        }
        private List<DNA<Tuple<long, float>>> PopulateFirstPopulation()
        {
            List<DNA<Tuple<long, float>>> firstPopulation = new List<DNA<Tuple<long, float>>>();

            DNA<Tuple<long, float>> previousBest = new DNA<Tuple<long, float>>(optModelMap.Count, random, 
													GetRandomGene, FitnessFunction, MutateFunction, shouldInitGenes: false);
													previousBest.Genes = new Tuple<long, float>[optModelMap.Count];

            for (int i = 0; i < NUMBER_OF_POPULATION; i++)
            {
				DNA<Tuple<long, float>> dna1 = new DNA<Tuple<long, float>>(optModelMap.Count, random, GetRandomGene, FitnessFunction, MutateFunction, shouldInitGenes: true);
				ScaleDNA(dna1, -1);
				firstPopulation.Add(dna1);
            }

            return firstPopulation;
        }
        private float CalculateCost(Tuple<long, float>[] genes)
        {
            float cost = 0;
            foreach (Tuple<long, float> gene in genes)
            {
                float price = optModelMap[gene.Item1].CalculatePrice(gene.Item2);
                optModelMap[gene.Item1].Price = price;
                cost += price;
            }

            return cost;
        }

		private float CalculateCost()
		{
			float cost = 0;
			foreach (var item in optModelMap)
			{
				float price = item.Value.CalculatePrice(item.Value.GenericOptimizedValue);
				item.Value.Price = price;
				cost += price;
			}

			return cost;
		}

		private DNA<Tuple<long, float>> ScaleDNA(DNA<Tuple<long,float>> dna, int mutatedGene)
		{
			List<dynamic> t = new List<dynamic>();
			dynamic mutatedGen = null;
			if (mutatedGene != -1)
			{
				mutatedGen = dna.Genes[mutatedGene];
			}
			dynamic sumOfDna = 0;

			for (int i = 0; i < dna.Size; i++)
			{
				if (i != mutatedGene)
				{
					t.Add(dna.Genes[i]);
					sumOfDna += dna.Genes[i].Item2;
				}
			}

			float k = (NecessaryEnergy - (mutatedGen?.Item2 ?? 0f)) / (float)sumOfDna;
			for (int i = 0; i < dna.Size; i++)
			{
				if (i != mutatedGene)
				{
					dna.Genes[i] = new Tuple<long, float>(dna.Genes[i].Item1, dna.Genes[i].Item2 * k);
				}
			}
			return dna;
		}

        private void SetAlgorithmOptions(int iteationsCount, int populationCount, int elit, float mutationRat)
        {
            ELITIMS_PERCENTAGE = elit;
            NUMBER_OF_ITERATION = iteationsCount;
            NUMBER_OF_POPULATION = populationCount;
            mutationRate = mutationRat;
        }
	}
}
