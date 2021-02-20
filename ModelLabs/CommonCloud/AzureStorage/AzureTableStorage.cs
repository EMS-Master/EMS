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
            catch(Exception e)
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
            catch(Exception e)
            {
                return null;
            }
        }


        public static List<HistoryMeasurement> GetAllHistoryMeasurementsForSelectedTime(string connectionString, string tableName, DateTime startTime, DateTime endTime, long gid)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference(tableName);

            TableContinuationToken token = null;
            List<HistoryMeasurement> entities = new List<HistoryMeasurement>();
            try
            {
                string filter1 = TableQuery.GenerateFilterConditionForLong("Gid", QueryComparisons.Equal, gid);
                string filter2 = TableQuery.GenerateFilterConditionForDate("MeasurementTime", QueryComparisons.GreaterThanOrEqual, startTime.ToUniversalTime());
                string filter3 = TableQuery.GenerateFilterConditionForDate("MeasurementTime", QueryComparisons.LessThanOrEqual, endTime.ToUniversalTime());

                string finalFilter = TableQuery.CombineFilters(TableQuery.CombineFilters(filter1, TableOperators.And, filter2),TableOperators.And, filter3);

                var query = new TableQuery<HistoryMeasurement>().Where(finalFilter);

                do
                {
                    var queryResult = table.ExecuteQuerySegmented(query, token);
                    entities.AddRange(queryResult.Results);
                    token = queryResult.ContinuationToken;
                } while (token != null);
            }
            catch(Exception e)
            {
           
            }

            return entities;
        }
        public static List<TotalProduction> GetAllTotalProductionsForSelectedTime(string connectionString, string tableName, DateTime startTime, DateTime endTime)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference(tableName);

            TableContinuationToken token = null;
            List<TotalProduction> entities = new List<TotalProduction>();

            try
            {
                string filter1 = TableQuery.GenerateFilterConditionForDate("TimeOfCalculation", QueryComparisons.GreaterThanOrEqual, startTime.ToUniversalTime());
                string filter2 = TableQuery.GenerateFilterConditionForDate("TimeOfCalculation", QueryComparisons.LessThanOrEqual, endTime.ToUniversalTime());

                string finalFilter = TableQuery.CombineFilters(filter1, TableOperators.And, filter2);

                var query = new TableQuery<TotalProduction>().Where(finalFilter);

                do
                {
                    var queryResult = table.ExecuteQuerySegmented(query, token);
                    entities.AddRange(queryResult.Results);
                    token = queryResult.ContinuationToken;
                } while (token != null);
            }
            catch (Exception e)
            {

            }
            return entities;
        }

    }
}
