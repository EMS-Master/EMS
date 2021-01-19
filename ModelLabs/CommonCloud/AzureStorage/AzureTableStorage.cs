using CommonCloud.AzureStorage.Entities;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonCloud.AzureStorage
{
    public static class AzureTableStorage
    {
        public static List<DiscreteCounter> GetAllDiscreteCounters(string connectionString, string tableName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference(tableName);

            TableContinuationToken token = null;
            List<DiscreteCounter> entities = new List<DiscreteCounter>();

            do
            {
                var queryResult = table.ExecuteQuerySegmented(new TableQuery<DiscreteCounter>(), token);
                entities.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            return entities;
        }

        public static bool AddTableEntityInDB<T>(T entity, string connectionString, string tableName) where T : TableEntity
        {
            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

                // Create the table client.
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                // Retrieve a reference to the table.
                CloudTable table = tableClient.GetTableReference(tableName);

                // Create the table if it doesn't exist.
                table.CreateIfNotExists();

                //Add Entity into table
                TableOperation insertOperation = TableOperation.InsertOrReplace(entity);

                table.Execute(insertOperation);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
