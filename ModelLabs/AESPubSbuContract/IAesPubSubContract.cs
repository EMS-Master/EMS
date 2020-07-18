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
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IAesPubSubCallbackContract))]
    public interface IAesPubSubContract
    {
        [OperationContract(IsOneWay = false, IsInitiating = true)]
        void Subscribe();

        [OperationContract(IsOneWay = false, IsInitiating = true)]
        void Unsubscribe();
    }

    public interface IAesPubSubCallbackContract
    {
        [OperationContract(IsOneWay = false)]
        void AlarmsEvents(Alarm alarm);

        [OperationContract(IsOneWay = false)]
        void UpdateAlarmsEvents(Alarm alarm);
    }
}
