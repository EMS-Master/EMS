using CalculationEngineService.GeneticAlgorithm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineService
{
    class Program
    {
        public static void Main(string[] args)
        {
            Stopwatch sw;
            sw = Stopwatch.StartNew();
            GA g = new GA();
            sw.Stop();
            Console.WriteLine("Time taken: {0}s", sw.Elapsed.Seconds);
            Console.ReadLine();
        }
    }
}
