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
	public class DataSimulatorService
	{
		private MdbClient mdbClient;
		private Dictionary<int, Tuple<int, int>> insolationRange;
		private float oldWindSpeed = 5f;
		 
		
		public DataSimulatorService()
		{
			ConnectToSimulator();
			PopulateInsolationRange();
		}

		private void ConnectToSimulator()
		{
			try
			{
				mdbClient = new MdbClient("localhost", 502);
				if (mdbClient.Connected)
				{
					return;
				}
				mdbClient.Connect("localhost", 502);
			}
			catch (SocketException e)
			{
				Thread.Sleep(2000);
				ConnectToSimulator();
			}
			catch (Exception)
			{
				throw;
			}
		}

		public void SimulateSunData()
		{
			TimeSpan time = DateTime.Now.TimeOfDay;
			var range = insolationRange.ContainsKey(time.Hours) ? insolationRange[time.Hours] : new Tuple<int, int>(0,0);
			var random = new Random();
			float insolation = (float)random.NextDouble() + random.Next(range.Item1, range.Item2);
			float powerForGenerator1 = (random.Next(95, 105) / 100f) * insolation;
			float powerForGenerator2 = (random.Next(95, 105) / 100f) * insolation;
			float powerForGenerator3 = (random.Next(95, 105) / 100f) * insolation;

			mdbClient.WriteSingleRegister(49, insolation);
			mdbClient.WriteSingleRegister(20, powerForGenerator1);
			mdbClient.WriteSingleRegister(22, powerForGenerator2);
			mdbClient.WriteSingleRegister(24, powerForGenerator3);

			Console.WriteLine(insolation);
			Console.WriteLine(powerForGenerator1);
			Console.WriteLine(powerForGenerator2);
			Console.WriteLine(powerForGenerator3);
			Console.WriteLine();
			
		}

		public void SimulateWindData()
		{
			Random random = new Random();
			float windSpeed = random.Next(80, 120) / 100f * oldWindSpeed;
			float powerGenerator1 = 0;
			float powerGenerator2 = 0;
			float powerGenerator3 = 0;

			float k1 = 932.896f;
			float k2 = 1119.475f;
			float k3 = 597.053f;

			if(windSpeed > 35)
			{
				windSpeed = random.Next(80, 82) / 100f * windSpeed;
			}

			if(windSpeed >= 3.61 && windSpeed <= 13.89)
			{
				powerGenerator1 = k1 * (float)Math.Pow(windSpeed, 3);
				powerGenerator2 = k2 * (float)Math.Pow(windSpeed, 3);
				powerGenerator3 = k3 * (float)Math.Pow(windSpeed, 3);
			}
			else if(windSpeed < 3.61 || windSpeed > 27.78)
			{
				powerGenerator1 = 0;
				powerGenerator2 = 0;
				powerGenerator3 = 0;
			}
			else
			{
				powerGenerator1 = k1 * (float)Math.Pow(13.89f, 3);
				powerGenerator2 = k2 * (float)Math.Pow(13.89f, 3);
				powerGenerator3 = k3 * (float)Math.Pow(13.89f, 3);
			}

			Console.WriteLine("Wind data");
			Console.WriteLine(windSpeed);
			Console.WriteLine(powerGenerator1/1000000f);	//W -> MW
			Console.WriteLine(powerGenerator2/1000000f);
			Console.WriteLine(powerGenerator3/1000000f);
			Console.WriteLine();

			mdbClient.WriteSingleRegister(51, windSpeed);
			mdbClient.WriteSingleRegister(26, powerGenerator1 / 1000000f);
			mdbClient.WriteSingleRegister(28, powerGenerator2 / 1000000f);
			mdbClient.WriteSingleRegister(30, powerGenerator3 / 1000000f);

			oldWindSpeed = windSpeed;
		}

		private void PopulateInsolationRange()
		{
			insolationRange = new Dictionary<int, Tuple<int, int>>();

			insolationRange.Add(5, new Tuple<int, int>(10, 70));
			insolationRange.Add(6, new Tuple<int, int>(70, 200));
			insolationRange.Add(7, new Tuple<int, int>(200, 400));
			insolationRange.Add(8, new Tuple<int, int>(400, 600));
			insolationRange.Add(9, new Tuple<int, int>(600, 780));
			insolationRange.Add(10, new Tuple<int, int>(780, 900));
			insolationRange.Add(11, new Tuple<int, int>(900, 1000));
			insolationRange.Add(12, new Tuple<int, int>(900, 1000));
			insolationRange.Add(13, new Tuple<int, int>(780, 900));
			insolationRange.Add(14, new Tuple<int, int>(600, 780));
			insolationRange.Add(15, new Tuple<int, int>(450, 600));
			insolationRange.Add(16, new Tuple<int, int>(250, 450));
			insolationRange.Add(17, new Tuple<int, int>(100, 250));
			insolationRange.Add(18, new Tuple<int, int>(7, 100));
		}
	}
}
