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
        void AddAlarm(AlarmHelper alarm);

        //send measured value to alarmEvents service(gid, value)
        [OperationContract]
        void UpdateStatus(AnalogLocation analogLoc, State state);

        [OperationContract]
        void UpdateAckStatus(AlarmHelper alarmtoupdate);
    }
}
