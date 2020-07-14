﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;


namespace FTN.ServiceContracts
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(ICePubSubCallbackContract))]
    public interface ICePubSubContract
    {
        [OperationContract(IsOneWay = false, IsInitiating = true)]
        void Subscribe();

        //[OperationContract(IsOneWay = false, IsInitiating = true)]
        //bool ChooseOptimization(OptimizationType optimizationType);

        [OperationContract(IsOneWay = false, IsTerminating = true)]
        void Unsubscribe();
    }

    public interface ICePubSubCallbackContract
    {
        [OperationContract(IsOneWay = false)]
        void OptimizationResults(List<MeasurementUI> result);
    }
}