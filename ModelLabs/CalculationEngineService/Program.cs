using CalculationEngineServ;
using FTN.Common;
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
       // public static void Main(string[] args)
        //{
        //    Stopwatch sw;
        //    sw = Stopwatch.StartNew();
        //    GA g = new GA();
        //    sw.Stop();
        //    Console.WriteLine("Time taken: {0}s", sw.Elapsed.Seconds);
        //    Console.ReadLine();
       // }


        private static void Main(string[] args)
        {
            using (var db = new AlarmContext())
            {

                db.Alarms.Add(new Alarm { Gid = 1, AlarmValue = 300, MinValue = 150, MaxValue = 250, AckState = 0, AlarmType = 1, AlarmMessage = "aaa" });


                db.SaveChanges();

                //foreach (var alarm in db.Alarms)
                //{
                //    Console.WriteLine(alarm.Gid);
                //}
            }
            StartStandard(); 
        }


        private static void StartStandard()
        {
            try
            {
                string message = "Starting Calculation Engine Service...";
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
                Console.WriteLine("\n{0}\n", message);

                using (CalculationEngineServiceClass ces = new CalculationEngineServiceClass())
                {
                    ces.Start();

                  

                    message = "Press <Enter> to stop the service.";
                    CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
                    Console.WriteLine(message);
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("CalculationEngineService failed.");
                Console.WriteLine(ex.StackTrace);
                CommonTrace.WriteTrace(CommonTrace.TraceError, ex.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, "CalculationEngineService failed.");
                CommonTrace.WriteTrace(CommonTrace.TraceError, ex.StackTrace);
                Console.ReadLine();
            }
        }

    }
}
