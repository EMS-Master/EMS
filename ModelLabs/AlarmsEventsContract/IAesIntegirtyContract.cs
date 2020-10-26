using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using CommonMeas;
using CalculationEngineServ.DataBaseModels;

namespace FTN.ServiceContracts
{
   [ServiceContract]
   public interface IAesIntegirtyContract
    {
        //initiates integrity update
        [OperationContract]
        List<AlarmHelper> InitiateIntegrityUpdate();
    }
}
