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
    public interface IAlarmsEventsContract
    {
        //add new alarm
        [OperationContract]
        void AddAlarm(Alarm alarm);

        //send measured value to alarmEvents service(gid, value)
        [OperationContract]
        void UpdateStatus(Alarm analogLoc, State state);
    }
}
