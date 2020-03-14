using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineServ.GeneticAlgorithm
{
    public class DNA<T>
    {
        public T[] Genes { get; set; }
        public float Fitness { get; private set; }

        private Random random;
        private Func<int, T> getRandomGene;
        private Func<DNA<T>, float> fitnessFunction;
        private Func<T, float, T> mutateFunction;

        public DNA(int size, Random random, Func<int, T> getRandomGene, Func<DNA<T>, float> fitnessFunction, Func<T, float, T> mutateFunction, bool shouldInitGenes = true)
        {
            Genes = new T[size];
            this.random = random;
            this.getRandomGene = getRandomGene;
            this.fitnessFunction = fitnessFunction;
            this.mutateFunction = mutateFunction;

            if (shouldInitGenes)
            {
                for (int i = 0; i < Genes.Length; i++)
                {
                    Genes[i] = getRandomGene(i);
                }
            }
        }

        public float CalculateFitness()
        {
            Fitness = fitnessFunction(this);
            return Fitness;
        }

        public DNA<T> Crossover(DNA<T> otherParent)
        {
            DNA<T> child = new DNA<T>(Genes.Length, random, getRandomGene, fitnessFunction, mutateFunction, shouldInitGenes: false);

            for (int i = 0; i < Genes.Length; i++)
            {
                child.Genes[i] = random.NextDouble() < 0.5 ? Genes[i] : otherParent.Genes[i];
            }

            return child;
        }

        public void Mutate(float mutationRate)
        {
            for (int i = 0; i < Genes.Length; i++)
            {
                Genes[i] = mutateFunction(Genes[i], mutationRate);
            }
        }
    }
}
