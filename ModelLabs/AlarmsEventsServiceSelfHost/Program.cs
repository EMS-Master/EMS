using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using FTN.Common;
using CalculationEngineService;
using CommonMeas;

namespace FTN.Services.AlarmsEventsService
{
   public class Program
    {
      private  static void Main(string[] args)
        {
            try
            {                
                string message = "Starting Alarms Events Service...";
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
                Console.WriteLine("\n{0}\n", message);

                using (FTN.Services.AlarmsEventsService.AlarmsEventsService aes = new FTN.Services.AlarmsEventsService.AlarmsEventsService())
                {
                    aes.Start();

                    message = "Press <Enter> to stop the service.";
                    CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
                    Console.WriteLine(message);
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("AlarmsEventsService failed.");
                Console.WriteLine(ex.StackTrace);
                CommonTrace.WriteTrace(CommonTrace.TraceError, ex.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, "AlarmsEventsService failed.");
                CommonTrace.WriteTrace(CommonTrace.TraceError, ex.StackTrace);
                Console.ReadLine();
            }
        }
    }
}
