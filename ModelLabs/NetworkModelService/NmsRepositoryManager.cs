using CommonCloud.AzureStorage;
using FTN.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTN.Services.NetworkModelService
{
    public class NmsRepositoryManager : INmsRepositoryManager
    {
        public NmsRepositoryManager()
        {
        }

        public void AddDelta(byte[] delta)
        {
            AzureBlobStorage.AddBlobEntityInDB("UseDevelopmentStorage=true;", "blobcontainer", "blobDelta", delta);
        }

        public byte[] ReadDelta()
        {
            return AzureBlobStorage.ReadBlobEntityFromDB("UseDevelopmentStorage=true;", "blobcontainer", "blobDelta");
        }
    }
}
