﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using CommonMeas;

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

    /// Communication between service and client is duplex, so this interface represent a callback contract which
    /// the service uses to pass new information to subscribed list of clients.
    public interface IAesPubSubCallbackContract
    {
        [OperationContract(IsOneWay = false)]
        void AlarmsEvents(AlarmHelper alarm);

        [OperationContract(IsOneWay = false)]
        void UpdateAlarmsEvents(AlarmHelper alarm);
    }
}
