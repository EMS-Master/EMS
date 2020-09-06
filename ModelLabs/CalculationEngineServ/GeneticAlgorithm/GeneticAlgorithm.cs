using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineServ.GeneticAlgorithm
{
    public class GeneticAlgorithm<T>
    {
        public List<DNA<T>> Population { get; set; }
        public int Generation { get; private set; }
        public float BestFitness { get; private set; }
        public T[] BestGenes { get; private set; }
		public float NecessaryEnergy { get; set; }

        public int Elitism;
        public float MutationRate;

        private List<DNA<T>> newPopulation;
        private Random random;
        private float fitnessSum;
        private int dnaSize;
        private Func<int, T> getRandomGene;
        private Func<DNA<T>, float> fitnessFunction;
        private Func<T, float, T> mutateFunction;
		private Func<DNA<T>,int,DNA<T>> scaleDNA;

		public GeneticAlgorithm(int populationSize, int dnaSize, Random random, Func<int, T> getRandomGene, Func<DNA<T>, float> fitnessFunction,
            Func<T, float, T> mutateFunction, int elitism, float necessaryEnergy, Func<DNA<T>,int,DNA<T>> scaleDNA, float mutationRate = 0.01f, bool shoudlInitPopulation = true)
        {
            Generation = 1;
            Elitism = elitism;
            MutationRate = mutationRate;
            Population = new List<DNA<T>>(populationSize);
            newPopulation = new List<DNA<T>>(populationSize);
			NecessaryEnergy = necessaryEnergy;
			this.random = random;
            this.dnaSize = dnaSize;
            this.getRandomGene = getRandomGene;
            this.fitnessFunction = fitnessFunction;
            this.mutateFunction = mutateFunction;
			this.scaleDNA = scaleDNA;

            BestGenes = new T[dnaSize];

            if (shoudlInitPopulation)
            {
                for (int i = 0; i < populationSize; i++)
                {
                    Population.Add(new DNA<T>(dnaSize, random, getRandomGene, fitnessFunction, mutateFunction, shouldInitGenes: true));
                }
            }
        }
        internal T[] StartAndReturnBest(int numOfIterations)
        {
            for (int i = 0; i < numOfIterations; i++)
            {
                NewGeneration();
            }

            return BestGenes;
        }
        //dodavanje novih elemenata u generaciju
        public void NewGeneration()
        {
			int geneToMutate = random.Next(0, dnaSize - 1);
            if (Population.Count <= 0)
            {
                return;
            }
            else if(Population.Count > 0)
            {
                CalculateFitness();
                Population.Sort(CompareDNA);
            }
            newPopulation.Clear();
			int numberOfElite = Population.Count * Elitism / 100;
            for (int i = 0; i < Population.Count; i++)
            {                                          
                if (i < numberOfElite )
                {                          
                    newPopulation.Add(Population[i]);
                }
                else if (i < Population.Count - numberOfElite)
                {
					List<DNA<T>> listOfParents = ChooseParents();
                    DNA<T> parent1 = listOfParents[0];
                    DNA<T> parent2 = listOfParents[1];
                    DNA<T> child = null;

                    if (parent1 == null || parent2 == null)
                    {
                        child = new DNA<T>(dnaSize, random, getRandomGene, fitnessFunction, mutateFunction, shouldInitGenes: true);
                    }
                    else
                    {
                        child = parent1.Crossover(parent2);
                    }

                    child.Mutate(MutationRate, geneToMutate);
					scaleDNA(child, geneToMutate);
				
					newPopulation.Add(child);
                }
                else
                {
					DNA<T> dna1 = new DNA<T>(dnaSize, random, getRandomGene, fitnessFunction, mutateFunction, shouldInitGenes: true);
					scaleDNA(dna1, -1);
                    newPopulation.Add(dna1);
                }
            }
            
            List<DNA<T>> tmpList = Population;
            Population = newPopulation;
            newPopulation = tmpList;

            Generation++;
        }

        private int CompareDNA(DNA<T> a, DNA<T> b)
        {
            if (a.Fitness < b.Fitness)
            {
                return -1;
            }
            else if (a.Fitness > b.Fitness)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
      
        private void CalculateFitness()
        {
            fitnessSum = 0;
            DNA<T> best = Population[0];

            for (int i = 0; i < Population.Count; i++)
            {
                fitnessSum += Population[i].CalculateFitness();

                if (Population[i].Fitness < best.Fitness)
                {
                    best = Population[i];
                }
            }

            BestFitness = best.Fitness;
            best.Genes.CopyTo(BestGenes, 0);
        }

        private List<DNA<T>> ChooseParents()
        {
			int numberOfElite = Population.Count * Elitism / 100;
            int randomNumber1 = random.Next(1, numberOfElite);
			int randomNumber2 = 0;
			do
			{
				randomNumber2 = random.Next(1, numberOfElite);
			}
			while (randomNumber2 == randomNumber1);

			return new List<DNA<T>>
			{
				Population[randomNumber1],
				Population[randomNumber2]
			};
        }
    }
}
