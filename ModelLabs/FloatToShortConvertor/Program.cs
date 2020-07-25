using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FloatToShortConvertor
{
	class Program
	{
		static void Main(string[] args)
		{
			
			Console.WriteLine("------------------------------");
			while (true)
			{
				Console.ForegroundColor = ConsoleColor.White;
				float vIn = 0;
				Console.WriteLine("Enter float value: ");
				float.TryParse(Console.ReadLine(), out vIn);
				if (vIn == 0)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Value must be type of float.");
					//	Console.WriteLine();
					Console.ForegroundColor = ConsoleColor.White;
					Console.WriteLine("------------------------------");
					
					continue;
				}

				byte[] bytes = BitConverter.GetBytes(vIn);
				
				byte[] up = new byte[] { bytes[1], bytes[0]};
				byte[] lo = new byte[] { bytes[3], bytes[2]};

				short upper = BitConverter.ToInt16(up, 0);
				short lower = BitConverter.ToInt16(lo, 0);


				Console.Write("Upper register: ");
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine(upper);
				Console.ForegroundColor = ConsoleColor.White;
				
				Console.Write("Lower register: ");
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine(lower);
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine("------------------------------");

			}
		}
	}
}
