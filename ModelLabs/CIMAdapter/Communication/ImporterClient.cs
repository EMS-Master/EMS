using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using TransactionContract;

namespace FTN.ESI.SIMES.CIM.CIMAdapter.Communication
{
    public class ImporterClient : ClientBase<IImporterContract>, IImporterContract
    {
        public ImporterClient() { }
        public ImporterClient(string Endpoint) : base(Endpoint) { }
        public UpdateResult ModelUpdate(Delta delta)
        {
            return Channel.ModelUpdate(delta);
        }
    }
}
