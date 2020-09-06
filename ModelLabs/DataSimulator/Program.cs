﻿using ModbusClient;
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
					lock (lockObj)
					{
						float sunGeneration = dss.SimulateSunData();
						float windGeneration = dss.SimulateWindData();
						float hydroGeneration = dss.SimulateHydroData();
						dss.SimulateConsumption(sunGeneration, windGeneration, hydroGeneration);
					}
					Thread.Sleep(10000);
				}
			});
			task.Start();
			
			Console.ReadLine();
		}
	}
}
