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
        private Dictionary<int, Tuple<double, double>> consumptionRange;
        private float oldWindSpeed = 5f;
		 
		
		public DataSimulatorService()
		{
			ConnectToSimulator();
			PopulateInsolationRange();
            PopulateConsumptionRange();

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

        public void SimulateConsumption()
        {
            TimeSpan time = DateTime.Now.TimeOfDay;
            var range = consumptionRange[time.Hours];

            Random random = new Random();

            float consumption1 = random.Next((int)(range.Item1 * 100), (int)(range.Item2 * 100))/100f;
            float consumption2 = random.Next((int)(range.Item1 * 100), (int)(range.Item2 * 100))/100f;
            float consumption3 = random.Next((int)(range.Item1 * 100), (int)(range.Item2 * 100))/100f;
            float consumption4 = random.Next((int)(range.Item1 * 100), (int)(range.Item2 * 100))/100f;
            float consumption5 = random.Next((int)(range.Item1 * 100), (int)(range.Item2 * 100))/100f;
            float consumption6 = random.Next((int)(range.Item1 * 100), (int)(range.Item2 * 100))/100f;
            float consumption7 = random.Next((int)(range.Item1 * 100), (int)(range.Item2 * 100))/100f;
            float consumption8 = random.Next((int)(range.Item1 * 100), (int)(range.Item2 * 100))/100f;
            float consumption9 = random.Next((int)(range.Item1 * 100), (int)(range.Item2 * 100))/100f;
            float consumption10 = random.Next((int)(range.Item1 * 100), (int)(range.Item2 * 100))/100f;


            mdbClient.WriteSingleRegister(0, consumption1); //W
            mdbClient.WriteSingleRegister(2, consumption2); //W
            mdbClient.WriteSingleRegister(4, consumption3); //W
            mdbClient.WriteSingleRegister(6, consumption4); //W
            mdbClient.WriteSingleRegister(8, consumption5); //W
            mdbClient.WriteSingleRegister(10, consumption6); //W
            mdbClient.WriteSingleRegister(12, consumption7); //W
            mdbClient.WriteSingleRegister(14, consumption8); //W
            mdbClient.WriteSingleRegister(16, consumption9); //W
            mdbClient.WriteSingleRegister(18, consumption10); //W

            Console.WriteLine("Consumption:");
            Console.WriteLine(consumption1);
            Console.WriteLine(consumption2);
            Console.WriteLine(consumption3);
            Console.WriteLine(consumption4);
            Console.WriteLine(consumption5);
            Console.WriteLine(consumption6);
            Console.WriteLine(consumption7);
            Console.WriteLine(consumption8);
            Console.WriteLine(consumption9);
            Console.WriteLine(consumption10);


        }

        private void PopulateConsumptionRange()
        {
            consumptionRange = new Dictionary<int, Tuple<double, double>>();

            consumptionRange.Add(0, new Tuple<double, double>(0, 0.3));
            consumptionRange.Add(1, new Tuple<double, double>(0, 0.26));
            consumptionRange.Add(2, new Tuple<double, double>(0, 0.2));
            consumptionRange.Add(3, new Tuple<double, double>(0, 0.18));
            consumptionRange.Add(4, new Tuple<double, double>(0.2, 0.31));
            consumptionRange.Add(5, new Tuple<double, double>(0.2, 0.35));
            consumptionRange.Add(6, new Tuple<double, double>(0.2, 0.4));
            consumptionRange.Add(7, new Tuple<double, double>(0.3, 0.45));
            consumptionRange.Add(8, new Tuple<double, double>(0.3, 0.5));
            consumptionRange.Add(9, new Tuple<double, double>(0.35, 0.5));
            consumptionRange.Add(10, new Tuple<double, double>(0.5, 0.61));
            consumptionRange.Add(11, new Tuple<double, double>(0.5, 0.62));
            consumptionRange.Add(12, new Tuple<double, double>(0.5, 0.63));
            consumptionRange.Add(13, new Tuple<double, double>(0.5, 0.65));
            consumptionRange.Add(14, new Tuple<double, double>(0.5, 0.67));
            consumptionRange.Add(15, new Tuple<double, double>(0.5, 0.7));
            consumptionRange.Add(16, new Tuple<double, double>(0.6, 0.9));
            consumptionRange.Add(17, new Tuple<double, double>(0.6, 0.92));
            consumptionRange.Add(18, new Tuple<double, double>(0.6, 0.94));
            consumptionRange.Add(19, new Tuple<double, double>(0.6, 0.98));
            consumptionRange.Add(20, new Tuple<double, double>(0.8, 1));
            consumptionRange.Add(21, new Tuple<double, double>(0.8, 1));
            consumptionRange.Add(22, new Tuple<double, double>(0.8, 0.98));
            consumptionRange.Add(23, new Tuple<double, double>(0.8, 0.98));
        }

        public void TurnOnRenewableGenerators()
        {
            for(int i = 0; i<16; i++)
            {
                mdbClient.WriteSingleCoil((ushort)i, true);
            }
            
        }
	}
}
