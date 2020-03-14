using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineService.GeneticAlgorithm
{
    public class GA
    {
        private readonly int ELITIMS_PERCENTAGE = 5;
        private readonly int NUMBER_OF_ITERATION = 200;
        private readonly int NUMBER_OF_POPULATION = 100;
        private readonly float mutationRate = 1f;
        private Random random;
        private GeneticAlgorithm<Tuple<long, float>> ga;

        public GA()
        {
            random = new Random();
            ga = new GeneticAlgorithm<Tuple<long, float>>(NUMBER_OF_POPULATION, 13, random, GetRandomGene, FitnessFunction,
                                             MutateFunction, ELITIMS_PERCENTAGE, mutationRate);
            ga.StartAndReturnBest(NUMBER_OF_ITERATION);
        }

        private Tuple<long, float> GetRandomGene(int index)
        {
            long a = random.Next(0, 100);
            float b = random.Next(0, 100);

            return new Tuple<long, float>(a, b);

        }
        private float FitnessFunction(DNA<Tuple<long, float>> dna)
        {
            //izracunavanje najmanje cijene
            return 1000;
        }
        private Tuple<long, float> MutateFunction(Tuple<long, float> gene, float mutateRate)
        {
            long id = gene.Item1;

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

            return new Tuple<long, float>(id, mutatedGeneValue);
        }
    }
}
