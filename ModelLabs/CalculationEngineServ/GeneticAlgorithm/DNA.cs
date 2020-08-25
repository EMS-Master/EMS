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
        public int Size { get; set; }
        public float Fitness { get; private set; }

        private Random random;
        private Func<int, T> getRandomGene;
        private Func<DNA<T>, float> fitnessFunction;
        private Func<T, float, T> mutateFunction;

        public DNA(int size, Random random, Func<int, T> getRandomGene, Func<DNA<T>, float> fitnessFunction, Func<T, float, T> mutateFunction, bool shouldInitGenes = true)
        {
            Genes = new T[size];
            Size = size;
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
                //child.Genes[i] = ((Genes[i] + otherParent.Genes[i]) / 2);
            }
            return child;
        }

        public void Mutate(float mutationRate)
        {
            int loop = random.Next(1, Size-1);
            int index = 0;

            for (int i = 0; i < loop; i++)
            {
                index = random.Next(0, loop);
                Genes[index] = mutateFunction(Genes[index], mutationRate);
            }
        }
    }
}
