using ScadaService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScadaServiceSelfHost
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using(ScadaService.ScadaService scada = new ScadaService.ScadaService())
                {
                    scada.Start();
                    scada.StartCollectingData();
                    Console.ReadLine();
                    
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Scada service failed");
            }
        }
    }
}
