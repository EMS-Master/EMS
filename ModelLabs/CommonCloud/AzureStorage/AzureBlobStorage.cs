using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonCloud.AzureStorage
{
    public static class AzureBlobStorage
    {
        public static bool AddBlobEntityInDB(string connectionString, string containerName, string blobName, byte[] byteArray)
        {
            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

                CloudBlobClient tableClient = storageAccount.CreateCloudBlobClient();

                CloudBlobContainer container = tableClient.GetContainerReference(containerName);
                container.CreateIfNotExists();
                CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
                blob.UploadFromByteArray(byteArray, 0, byteArray.Count());

                return true;

            }
            catch
            {
                return false;
            }
        }

        public static byte[] ReadBlobEntityFromDB(string connectionString, string containerName, string blobName)
        {
            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                CloudBlobContainer container = blobClient.GetContainerReference(containerName);
                if(container == null)
                {
                    return null;
                }
                CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
                //string p = "C:\\Users\\barba\\Desktop\\EMS\\ModelLabs\\";
                //string filename = "NetworkModelData1.data";
                //string pa = Path.Combine(p, filename);

                using (var ms = new MemoryStream())
                {
                    blob.DownloadToStream(ms);
                    return ms.ToArray();
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

    }
}
