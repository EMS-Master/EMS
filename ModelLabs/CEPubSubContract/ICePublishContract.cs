using FTN.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace EMS.ServiceContracts
{
    [ServiceContract]
    public interface ICePublishContract
    {
        [OperationContract]
        void PublishOptimizationResults(List<MeasurementUI> result);
    }
}
