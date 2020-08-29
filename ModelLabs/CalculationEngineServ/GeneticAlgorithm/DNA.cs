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
			}
			return child;

   //         DNA<T> child = new DNA<T>(Genes.Length, random, getRandomGene, fitnessFunction, mutateFunction, shouldInitGenes: false);
			//dynamic ch = child;
			//Tuple<long, float> chTuple = ch;
   //         for (int i = 0; i < Genes.Length; i++)
   //         {
			//	dynamic t1 = Genes[i];
			//	Tuple<long, float> t2 = t1;

			//	dynamic t3 = otherParent.Genes[i];
			//	Tuple<long, float> t4 = t1;

			//	float genes1 = t2.Item2;
			//	float genes2 = t4.Item2;
				
			//	float retVal = (genes1 + genes2) / 2;
			//	chTuple = new Tuple<long, float>(chTuple.Item1, retVal);

			//}
   //         return (dynamic)chTuple;
        }

        public void Mutate(float mutationRate, int geneToMutate)
        {
			Genes[geneToMutate] = mutateFunction(Genes[geneToMutate], mutationRate);
		}
    }
}
