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
                Console.WriteLine("Choose option: ");
                Console.WriteLine("1. Convert float to short ");
                Console.WriteLine("2. Convert short to float ");
                int selection = 0;
                Int32.TryParse(Console.ReadLine(), out selection);

                if(selection == 1)
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

                    byte[] up = new byte[] { bytes[1], bytes[0] };
                    byte[] lo = new byte[] { bytes[3], bytes[2] };

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
                else if(selection == 2)
                {
                    short vIn1 = 0;
                    short vIn2 = 0;
                    Console.WriteLine("Enter first short value: ");
                    short.TryParse(Console.ReadLine(), out vIn1);
                    if (vIn1 == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Value must be type of short.");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("------------------------------");

                        continue;
                    }
                    Console.WriteLine("Enter second short value: ");
                    short.TryParse(Console.ReadLine(), out vIn2);
                    if (vIn2 == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Value must be type of short.");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("------------------------------");

                        continue;
                    }
                    byte[] vOut1 = BitConverter.GetBytes(vIn1);
                    byte[] vOut2 = BitConverter.GetBytes(vIn2);

                    byte[] vOutZ = new byte[] { vOut1[1], vOut1[0], vOut2[1], vOut2[0] };
                    float vOutG = BitConverter.ToSingle(vOutZ, 0);
                    Console.Write("Float value is: ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(vOutG);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("------------------------------");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("You must enter 1 or 2");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("------------------------------");

                    continue;
                }
                
			}
		}
	}
}
