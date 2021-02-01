using Azure.Storage.Blobs;
using CommonCloud.AzureStorage.Entities;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.WindowsAzure.Storage.Blob;
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

        public static List<Alarm> GetAllAlarms(string connectionString, string tableName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference(tableName);

            TableContinuationToken token = null;
            List<Alarm> entities = new List<Alarm>();

            do
            {
                var queryResult = table.ExecuteQuerySegmented(new TableQuery<Alarm>(), token);
                entities.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            return entities;
        }

        public static List<CommandedGenerator> GetAllCommandedGenerators(string connectionString, string tableName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference(tableName);

            TableContinuationToken token = null;
            List<CommandedGenerator> entities = new List<CommandedGenerator>();

            do
            {
                var queryResult = table.ExecuteQuerySegmented(new TableQuery<CommandedGenerator>(), token);
                entities.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            return entities;
        }

        public static List<HistoryMeasurement> GetAllHistoryMeasurements(string connectionString, string tableName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference(tableName);

            TableContinuationToken token = null;
            List<HistoryMeasurement> entities = new List<HistoryMeasurement>();

            do
            {
                var queryResult = table.ExecuteQuerySegmented(new TableQuery<HistoryMeasurement>(), token);
                entities.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            return entities;
        }

        public static List<TotalProduction> GetAllTotalProductions(string connectionString, string tableName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference(tableName);

            TableContinuationToken token = null;
            List<TotalProduction> entities = new List<TotalProduction>();

            do
            {
                var queryResult = table.ExecuteQuerySegmented(new TableQuery<TotalProduction>(), token);
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

        public static bool InsertEntitiesInDB<T>(List<T> entities, string connectionString, string tableName) where T : TableEntity
        {
            if (entities.Count == 0)
                return false;

            try
            {
                // Retrieve the storage account from the connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

                // Create the table client.
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                // Create the CloudTable object that represents the "people" table.
                CloudTable table = tableClient.GetTableReference(tableName);

                // Create the table if it doesn't exist.
                table.CreateIfNotExists();

                // Create the batch operation.
                TableBatchOperation batchOperation = new TableBatchOperation();

                entities.ForEach(x => {
                    batchOperation.InsertOrReplace(x);
                });

                // Execute the batch operation.
                table.ExecuteBatch(batchOperation);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static DynamicTableEntity GetSingleEntityFromDB<T>(string partitionKey, string rowKey, string connectionString, string tableName) where T : TableEntity
        {
            try
            {
                // Retrieve the storage account from the connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

                // Create the table client.
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                // Create the CloudTable object that represents the "people" table.
                CloudTable table = tableClient.GetTableReference(tableName);

                // Create a retrieve operation that takes a customer entity.
                TableOperation retrieveOperation = TableOperation.Retrieve(partitionKey, rowKey);

                // Execute the retrieve operation.
                TableResult retrievedResult = table.Execute(retrieveOperation);

                if (retrievedResult != null)
                {
                    // Print the phone number of the result.
                    return (DynamicTableEntity)retrievedResult.Result;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

    }
}
