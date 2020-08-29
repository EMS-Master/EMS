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

		public float SimulateSunData()
		{
            // u 12 popodne sunce je pod uglom od 90 stepeni nad povrsinom. U tom trenutku insolacija je 1000 W/m na kvadrat
            // koeficijent je srazmeran Povrsina panela / alfa (ugao pod kojim je postavljen panel)
            // u nasem slucaju bice 15% tj. 0.15
            // pa je snaga u 12h sa insolacijom od 1000 jednaka 1000 * 0.15 = 150 W za jedan panel
            // Solarni generatori se razlikuju po broju panela
            // Imamo 5 solarnih generatora:
            // 1. 1000000 W = 1000 kW = 1 MW  = 6666 metara kvadratnih panela
            // 2. 1200000 W = 1200 kW = 1.2 MW = 8000 metara kvadratnih panela
            // 3. 1500000 W = 1500 kW = 1.5 MW = 10000 metara kvadratnih panela
            // 4. 1800000 W = 1800 kW = 1.8 MW = 12000 metara kvadratnih panela
            // 5. 1000000 W = 1000 kW = 1MW = 6666 metara kvadratnih panela

            float k = 0.15f; //koeficijent
            TimeSpan time = DateTime.Now.TimeOfDay;
			var range = insolationRange.ContainsKey(time.Hours) ? insolationRange[time.Hours] : new Tuple<int, int>(0,0);
			var random = new Random();
			float insolation = (float)random.NextDouble() + random.Next(range.Item1, range.Item2); // insolacija u datom trenutku
            float powerPerPV = insolation * k;
           
            float powerForGenerator1 = ((random.Next(95, 105) / 100f) * powerPerPV * 6666) / 1000;
			float powerForGenerator2 = ((random.Next(95, 105) / 100f) * powerPerPV * 80000)/1000;
			float powerForGenerator3 = ((random.Next(95, 105) / 100f) * powerPerPV * 10000)/1000;
            float powerForGenerator4 = ((random.Next(95, 105) / 100f) * powerPerPV * 12000)/1000;
            float powerForGenerator5 = ((random.Next(95, 105) / 100f) * powerPerPV * 6666 )/1000;

            if (range.Item1 == 0 && range.Item2 == 0)
            {
                powerForGenerator1 = 0;
                powerForGenerator2 = 0;
                powerForGenerator3 = 0;
                powerForGenerator4 = 0;
                powerForGenerator5 = 0;
            }

            mdbClient.WriteSingleRegister(100, insolation);
			mdbClient.WriteSingleRegister(40, powerForGenerator1);
			mdbClient.WriteSingleRegister(42, powerForGenerator2);
			mdbClient.WriteSingleRegister(44, powerForGenerator3);
            mdbClient.WriteSingleRegister(46, powerForGenerator4);
            mdbClient.WriteSingleRegister(48, powerForGenerator5);

            Console.WriteLine("Insolation: " + insolation);
            Console.WriteLine("PoverPerPV: " + powerPerPV);
			Console.WriteLine("Sun generator 1: " +  powerForGenerator1 + " KW");
			Console.WriteLine("Sun generator 2: " +  powerForGenerator2 + " KW");
			Console.WriteLine("Sun generator 3: " +  powerForGenerator3 + " KW");
            Console.WriteLine("Sun generator 4: " +  powerForGenerator4 + " KW");
            Console.WriteLine("Sun generator 5: " +  powerForGenerator5 + " KW");
            Console.WriteLine();

            float sum = powerForGenerator1 + powerForGenerator2 + powerForGenerator3 + powerForGenerator4 + powerForGenerator5;
            return sum;
		}

		public float SimulateWindData()
		{
            // Podrazumeva se da su sve vetrenjace istog precnika, na istom mestu postavljene, gustina vazduha i brzina vetra su iste
            // formula Pw = Vw na treci * povrsina turbina * gusttina vazduha
            // ~ Vw na treci * koeficijent
            // Jedna vetroturbina od 1MW : povrsina rotora 2827m na kvadrat
            // Cut-in speed: 3.6 m/s
            // Cut-out speed: 20 m/s
            // Rated wind speed: 12.5 m/s
            // gustina vazduha: 1.2 kg/m kubni
			Random random = new Random();
			float windSpeed = random.Next(80, 120) / 100f * oldWindSpeed;
			float powerGenerator1 = 0;
			float powerGenerator2 = 0;
			float powerGenerator3 = 0;
			float powerGenerator4 = 0;
			float powerGenerator5 = 0;

            // 1MW = k * (rated wind speed) na treci
            //  k = 1000000 W / (12.5) na treci
            //  k = 1000000 / 1953.125 = 521

			float k = 521f;
			
			if(windSpeed > 35)
			{
				windSpeed = random.Next(80, 82) / 100f * windSpeed;
			}

            float oneWindTurbine = k * (float)Math.Pow(windSpeed, 3);
            float oneRatedWindTurbine = k * (float)Math.Pow(12.5, 3);

            if (windSpeed >= 3.6 && windSpeed <= 12.5)
			{
				powerGenerator1 = oneWindTurbine * 5 / 1000; //KW
				powerGenerator2 = oneWindTurbine * 8 / 1000;
				powerGenerator3 = oneWindTurbine * 10 / 1000;
				powerGenerator4 = oneWindTurbine * 14 / 1000;
				powerGenerator5 = oneWindTurbine * 18 / 1000;
			}
			else if(windSpeed < 3.6 || windSpeed > 20)
			{
				powerGenerator1 = 0;
				powerGenerator2 = 0;
				powerGenerator3 = 0;
				powerGenerator4 = 0;
				powerGenerator5 = 0;
			}
			else
			{
				powerGenerator1 = oneRatedWindTurbine * 5 / 1000;
				powerGenerator2 = oneRatedWindTurbine * 8 / 1000;
				powerGenerator3 = oneRatedWindTurbine * 10 / 1000;
				powerGenerator4 = oneRatedWindTurbine * 14 / 1000;
				powerGenerator5 = oneRatedWindTurbine * 18 / 1000;
			}

			Console.WriteLine("Wind data");
			Console.WriteLine("Wind speed: " + windSpeed);
			Console.WriteLine("Wind gen 1: " +  powerGenerator1  + "KW");	
			Console.WriteLine("Wind gen 2: " +  powerGenerator2 + " KW");
			Console.WriteLine("Wind gen 3: " +  powerGenerator3 + " KW");
			Console.WriteLine("Wind gen 4: " +  powerGenerator4 + " KW");
			Console.WriteLine("Wind gen 5: " +  powerGenerator5 + " KW");
			Console.WriteLine();

			mdbClient.WriteSingleRegister(102, windSpeed);
			mdbClient.WriteSingleRegister(50, powerGenerator1);
			mdbClient.WriteSingleRegister(52, powerGenerator2);
			mdbClient.WriteSingleRegister(54, powerGenerator3);
			mdbClient.WriteSingleRegister(56, powerGenerator4);
			mdbClient.WriteSingleRegister(58, powerGenerator5);
            
			oldWindSpeed = windSpeed;
            float sum = powerGenerator1 + powerGenerator2 + powerGenerator3 + powerGenerator4 + powerGenerator5;
            return sum;
        }

        public float SimulateHydroData()
        {
            float powerGenerator1 = 100000; //KW
            float powerGenerator2 = 120000;
            float powerGenerator3 = 130000;

            mdbClient.WriteSingleRegister(60, powerGenerator1);
            mdbClient.WriteSingleRegister(62, powerGenerator2);
            mdbClient.WriteSingleRegister(64, powerGenerator3);

            Console.WriteLine("Hydro data");
            Console.WriteLine("Hydro gen 1: " + powerGenerator1 + " KW");
            Console.WriteLine("Hydro gen 2: " + powerGenerator2 + " KW");
            Console.WriteLine("Hydro gen 3: " + powerGenerator3 + " KW");
            Console.WriteLine();

            float sum = powerGenerator1 + powerGenerator2 + powerGenerator3;
            return sum;
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

        public void SimulateConsumption(float sunGeneration, float windGeneration, float hydroGeneration)
        {
            TimeSpan time = DateTime.Now.TimeOfDay;
            var range = consumptionRange[time.Hours];

            Random random = new Random();

            List<float> consumptions = new List<float>();
            Console.WriteLine("Consumption:");

            for(int i = 0; i < 20; i++)
            {
                consumptions.Add(random.Next((int)(range.Item1 * 10000), (int)(range.Item2 * 10000))); //kW indutries
            }

            float sumOfConsumption = consumptions.Sum();

            float renewableGeneration = (sunGeneration + windGeneration + hydroGeneration); //kW
            if(renewableGeneration > sumOfConsumption)
            {
                float difference = renewableGeneration - sumOfConsumption;
                float valueToAdd = difference + random.Next(500,1000);
                float valueToAddForEachConsumer = valueToAdd / 20;
                for(var i = 0; i < 20; i++)
                {
                    consumptions[i] += valueToAddForEachConsumer;
                }
            }

			float consumptionsSum = consumptions.Sum();


			for (int i = 0; i < 20; i++)
            { 
                mdbClient.WriteSingleRegister((ushort)(i * 2), consumptions[i]); // kW
                Console.WriteLine("Consumer " + (i+1) + ": " + consumptions[i] + " kW");
            }
            
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

        public void TurnOnConsumersAndGenerators()
        {
            for(int i = 0; i<40; i++)
            {
                mdbClient.WriteSingleCoil((ushort)i, true);
            }
            
        }
	}
}
