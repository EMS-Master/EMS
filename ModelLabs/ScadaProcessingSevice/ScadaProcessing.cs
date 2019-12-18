using ModbusClient;
using ScadaContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScadaProcessingSevice
{
    public class ScadaProcessing : IScadaProcessingContract
    {
        //data collected from simulator should be passed through 
        //scadaProcessing,from scada, to calculationEngine for optimization
        public bool SendValues(byte[] value)
        {
            throw new NotImplementedException();
        }
    }
}
