using FTN.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineServ.PubSub
{
   public class OptimizationEventArgs : EventArgs
    {
        private string message;
        private List<MeasurementUI> optimizationResult;

        public string Message { get { return message; } set { message = value; }}
        public List<MeasurementUI> OptimizationResult { get { return optimizationResult; } set { optimizationResult = value; } }


    }
}
