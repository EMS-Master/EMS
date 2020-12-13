using FTN.Common;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.ServiceContracts
{
    public interface IImporterAsyncContract : IService
    {
        Task<UpdateResult> ModelUpdate(Delta delta);
    }
}
