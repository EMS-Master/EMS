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
        private float windPercent;
        private Tuple<DateTime, float> renewableKW;

        private Tuple<string, float, float> coReduction;


        public string Message { get { return message; } set { message = value; }}
        public List<MeasurementUI> OptimizationResult { get { return optimizationResult; } set { optimizationResult = value; } }
        public float WindPercent { get { return windPercent; } set { windPercent = value; } }
        public Tuple<DateTime, float> RenewableKW { get { return renewableKW; } set { renewableKW = value; } }

        public Tuple<string, float, float> CoReduction { get { return coReduction; } set { coReduction = value; } }


    }
}
