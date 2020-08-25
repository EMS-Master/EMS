using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineServ.GeneticAlgorithm
{
    public class GA
    {
        private readonly int ELITIMS_PERCENTAGE = 5;
        private readonly int NUMBER_OF_ITERATION = 200;
        private readonly int NUMBER_OF_POPULATION = 100;
        private readonly float mutationRate = 1f;
        private Random random;
        private GeneticAlgorithm<Tuple<long, float>> ga;
        private Dictionary<long, OptimisationModel> optModelMap;
        private Dictionary<int, long> indexToGid;
        private float necessaryEnergy;
        public float TotalCost { get; private set; }
        public float GeneratedPower { get; private set; }
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
            float sum = CalculateEnergy(dna.Genes);
            float k = 0;
            if(sum > necessaryEnergy)
            {
                k = necessaryEnergy / sum;
                foreach(var item in dna.Genes)
                {
                    //item.Item2 = item.Item2 *  k;
                }
            }
            float rez = CalculateCost(dna.Genes);
            return rez;
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
        public Dictionary<long, OptimisationModel> StartAlgorithmWithReturn()
        {
            random = new Random();
            ga = new GeneticAlgorithm<Tuple<long, float>>(NUMBER_OF_POPULATION, optModelMap.Count, random, GetRandomGene, FitnessFunction, MutateFunction, ELITIMS_PERCENTAGE, mutationRate, false);
            ga.Population = PopulateFirstPopulation();
            Tuple<long, float>[] bestGenes = ga.StartAndReturnBest(NUMBER_OF_ITERATION);

            for (int i = 0; i < bestGenes.Length; i++)
            {
                optModelMap[indexToGid[i]].GenericOptimizedValue = bestGenes[i].Item2;
            }
            TotalCost = CalculateCost(bestGenes);
            GeneratedPower = CalculateEnergy(bestGenes);

            return optModelMap;
        }
        private List<DNA<Tuple<long, float>>> PopulateFirstPopulation()
        {
            List<DNA<Tuple<long, float>>> firstPopulation = new List<DNA<Tuple<long, float>>>();

            DNA<Tuple<long, float>> previousBest = new DNA<Tuple<long, float>>(optModelMap.Count, random, GetRandomGene, FitnessFunction, MutateFunction, shouldInitGenes: false);
            previousBest.Genes = new Tuple<long, float>[optModelMap.Count];

            for (int i = 0; i < optModelMap.Count; i++)
            {
                long gid = indexToGid[i];
                previousBest.Genes[i] = new Tuple<long, float>(gid, optModelMap[gid].MeasuredValue);
            }
            firstPopulation.Add(previousBest);

            for (int i = 1; i < NUMBER_OF_POPULATION; i++)
            {
                firstPopulation.Add(new DNA<Tuple<long, float>>(optModelMap.Count, random, GetRandomGene, FitnessFunction, MutateFunction, shouldInitGenes: true));
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
    }
}
