using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace TransactionContract
{
    [ServiceContract(CallbackContract = typeof(ITransactionCallback))]
    public interface ITransactionContract
    {
        [OperationContract(IsOneWay = false)]
        UpdateResult Prepare(ref Delta delta);

        [OperationContract(IsOneWay = false)]
        bool Commit();

        [OperationContract(IsOneWay = false)]
        bool Rollback();
    }

    public interface ITransactionCallback
    {
        [OperationContract(IsOneWay = true)]
        void Response(string message);
    }
}
