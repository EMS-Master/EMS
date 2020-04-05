using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionManagerService;

namespace TransactionManagerServiceSelfHost
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Transaction Manager Service";

            try
            {
                string message = "Starting Transaction Manager Service ...";
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
                Console.WriteLine("\n{0}\n", message);

                using (TransactionManagerService.TransactionManagerService tmService = new TransactionManagerService.TransactionManagerService())
                {
                    tmService.Start();

                    Console.WriteLine("Analog test");
                    
                    message = "Press <Enter> to stop the service.";
                    CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
                    Console.WriteLine(message);
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Transaction Manager Service failed.");
                Console.WriteLine(ex.StackTrace);
                CommonTrace.WriteTrace(CommonTrace.TraceError, ex.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, "Transaction Manager Service failed.");
                CommonTrace.WriteTrace(CommonTrace.TraceError, ex.StackTrace);

                Console.ReadLine();
            }
        }
    }
}
