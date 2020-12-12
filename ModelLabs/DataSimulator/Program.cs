using ModbusClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataSimulator
{
	class Program
	{
		static void Main(string[] args)
		{
			DataSimulatorService dss = new DataSimulatorService();
			object lockObj = new object();

			dss.TurnOnConsumersAndGenerators();
			Task task = new Task(() =>
			{
				while (true)
				{
					
					List<float> sunGeneration = dss.SimulateSunData();
                    List<float> windGeneration = dss.SimulateWindData();
                    List<float> hydroGeneration = dss.SimulateHydroData();
                    float generationSum = sunGeneration.Sum() + windGeneration.Sum() + hydroGeneration.Sum();
                    Console.WriteLine("Generation sum: " + generationSum);
                    List<float> consum = dss.SimulateConsumption(sunGeneration.Sum(), windGeneration.Sum(), hydroGeneration.Sum());
                    float consumSum = consum.Sum();
                    float diff = consumSum - generationSum;
                    try
                    {
                        dss.WriteToSimulatorEverything(sunGeneration, windGeneration, hydroGeneration, consum);

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }


                    Thread.Sleep(4000);
				}
			});
			task.Start();
			
			Console.ReadLine();
		}
	}
}
