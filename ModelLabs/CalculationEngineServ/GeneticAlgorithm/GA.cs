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
        private float necessaryEnergy;
        public float TotalCost { get; private set; }
        public float GeneratedPower { get; private set; }
        public float EmissionCO2 { get; private set; }
        public GA(float necessaryEnergy, Dictionary<long, OptimisationModel> optModelMap)
        {
            indexToGid = new Dictionary<int, long>();
            int i = 0;
            foreach (var valPair in optModelMap)
            {
                indexToGid.Add(i++, valPair.Key);
            }

            this.necessaryEnergy = necessaryEnergy;
            this.optModelMap = optModelMap;
        }

        private Tuple<long, float> GetRandomGene(int index)
        {
            long gid = indexToGid[index];
            var minPower = optModelMap[gid].MinPower;
            var maxPower = optModelMap[gid].MaxPower;
            float randNumb = (float)GetRandomNumber(minPower, maxPower);
            return new Tuple<long, float>(gid, randNumb);
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
            ga = new GeneticAlgorithm<Tuple<long, float>>(NUMBER_OF_POPULATION, 
				optModelMap.Count, random, GetRandomGene, FitnessFunction, MutateFunction, 
				ELITIMS_PERCENTAGE, necessaryEnergy, ScaleDNA, mutationRate, false);
            ga.Population = PopulateFirstPopulation();
            Tuple<long, float>[] bestGenes = ga.StartAndReturnBest(NUMBER_OF_ITERATION);

            for (int i = 0; i < bestGenes.Length; i++)
            {
                optModelMap[indexToGid[i]].GenericOptimizedValue = bestGenes[i].Item2;
				optModelMap[indexToGid[i]].measurementUnit.CurrentValue = bestGenes[i].Item2;
			}

            float emCO2 = CalculationEngine.CalculateCO2(optModelMap);

            EmissionCO2 = emCO2;

            TotalCost = CalculateCost(bestGenes);
            GeneratedPower = CalculateEnergy(bestGenes);

            Console.WriteLine("Necessery energy: " + necessaryEnergy);
            Console.WriteLine("Total power: " + GeneratedPower);

            return optModelMap;
        }
        private List<DNA<Tuple<long, float>>> PopulateFirstPopulation()
        {
            List<DNA<Tuple<long, float>>> firstPopulation = new List<DNA<Tuple<long, float>>>();

            DNA<Tuple<long, float>> previousBest = new DNA<Tuple<long, float>>(optModelMap.Count, random, 
				GetRandomGene, FitnessFunction, MutateFunction, shouldInitGenes: false);
            previousBest.Genes = new Tuple<long, float>[optModelMap.Count];

            //for (int i = 0; i < optModelMap.Count; i++)
            //{
            //    long gid = indexToGid[i];
            //    previousBest.Genes[i] = new Tuple<long, float>(gid, optModelMap[gid].MeasuredValue);
            //}
            //firstPopulation.Add(previousBest);

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
        private float CalculateEnergy(Tuple<long, float>[] genes)
        {
            float energySum = 0;

            foreach (var gene in genes)
            {
                energySum += gene.Item2;
            }

            return energySum;
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

			float k = (necessaryEnergy - (mutatedGen?.Item2 ?? 0f)) / (float)sumOfDna;
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
