using FTN.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CEPubSubContract
{
    [ServiceContract]
    public interface ICEPublishContract
    {
        [OperationContract]
        void OptimizationResults(List<MeasurementUI> result);

        [OperationContract]
        void WindPercentResult(float result);

        [OperationContract]
        void RenewableResult(Tuple<DateTime, float> renewableKW);

        [OperationContract]
        void PublishCoReduction(Tuple<string, float, float> tupla);
    }
}
