using ScadaProcessingSevice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScadaProcessingSelfHost
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string message = "Starting SCADA Processing Service ...";
                Console.WriteLine("\n{0}\n", message);

                using (ScadaProcSevice scadaProc = new ScadaProcSevice())
                {
                    scadaProc.Start();

                    try
                    {
                        bool integrityResult = scadaProc.IntegrityUpdate();
                        if (integrityResult)
                        {
                            message = "Integrity Update finished successfully.";
                            Console.WriteLine(message);
                        }
                    }
                    catch (Exception e)
                    {
                        message = "Integrity Update failed. " + e.Message;
                        Console.WriteLine(message);
                    }

                    message = "Press <Enter> to stop the service.";
                    Console.WriteLine(message);
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("SCADA Crunching Service failed.");
                Console.WriteLine(ex.StackTrace);
                Console.ReadLine();
            }
        }
    }
}
