using CommonCloud.AzureStorage;
using FTN.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NmsRepositoryManagerService
{
    public class NmsRepositoryManagerCloud : INmsRepositoryManager
    {
        public void AddDelta(byte[] delta)
        {
            ServiceEventSource.Current.Message("Method AddDelta executed...");
            AzureBlobStorage.AddBlobEntityInDB("UseDevelopmentStorage=true;", "blobcontainer", "blobDelta", delta);
        }

        public byte[] ReadDelta()
        {
            ServiceEventSource.Current.Message("Method ReadDelta executed...");
            return AzureBlobStorage.ReadBlobEntityFromDB("UseDevelopmentStorage=true;", "blobcontainer", "blobDelta");
        }
    }
}
