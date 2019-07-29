namespace Janono.WindowsAzure.Storage.Table
{
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Table;
    using System.Threading;

    public class AzureTableEntity : TableEntity
    {
    }

    public interface IAzureTableStorage<T> where T : AzureTableEntity, new()
    {
        Task Delete(string partitionKey, string rowKey);
        Task<T> GetItem(string partitionKey, string rowKey);
        Task<List<T>> GetList();
        Task<List<T>> GetList(string partitionKey);
        Task Insert(T item);
        Task Insert(List<T> items);

        Task InsertReplace(List<T> items);
        Task Update(T item);
    }

    public class AzureTableStorage<T> : IAzureTableStorage<T>
        where T : AzureTableEntity, new()
    {
        #region " Public "

        public AzureTableStorage(AzureTableSettings settings)
        {
            this.settings = settings;
        }

        public async Task<List<T>> GetList()
        {
            //Table
            CloudTable table = await GetTableAsync();

            //Query
            TableQuery<T> query = new TableQuery<T>();

            List<T> results = new List<T>();
            TableContinuationToken continuationToken = null;
            do
            {
                TableQuerySegment<T> queryResults =
                    await table.ExecuteQuerySegmentedAsync(query, continuationToken);

                continuationToken = queryResults.ContinuationToken;
                results.AddRange(queryResults.Results);

            } while (continuationToken != null);

            return results;
        }

        public async Task<List<T>> GetList(string partitionKey)
        {
            CloudTable table = await GetTableAsync();

            TableQuery<T> query = new TableQuery<T>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.Equal, partitionKey));

            List<T> results = new List<T>();
            TableContinuationToken continuationToken = null;
            do
            {
                TableQuerySegment<T> queryResults = await table.ExecuteQuerySegmentedAsync(query, continuationToken);

                continuationToken = queryResults.ContinuationToken;

                results.AddRange(queryResults.Results);

            } while (continuationToken != null);

            return results;
        }

        public async Task<T> GetItem(string partitionKey, string rowKey)
        {
            CloudTable table = await GetTableAsync();

            TableOperation operation = TableOperation.Retrieve<T>(partitionKey, rowKey);

            TableResult result = await table.ExecuteAsync(operation);

            return (T)(dynamic)result.Result;
        }

        public async Task<List<T>> Query(TableQuery<T> query, CancellationToken ct = default(CancellationToken))
        {
            CloudTable table = await GetTableAsync();

            var items = new List<T>();
            TableContinuationToken token = null;

            do
            {
                TableQuerySegment<T> seg = await table.ExecuteQuerySegmentedAsync<T>(query, token);
                token = seg.ContinuationToken;
                items.AddRange(seg);
            } while (token != null);

            return items;
        }

        public async Task Insert(T item)
        {
            CloudTable table = await GetTableAsync();

            TableOperation operation = TableOperation.Insert(item);

            await table.ExecuteAsync(operation);
        }

        public async Task Insert(List<T> items)
        {
            const int batchSize = TableConstants.TableServiceBatchMaximumOperations;

            CloudTable table = await GetTableAsync();

            while (items.Any())
            {
                TableBatchOperation batchOperation = new TableBatchOperation();
                var first100 = items.Take(batchSize).ToList();
                foreach (var tableOperation in first100)
                {
                    batchOperation.Insert(tableOperation);
                }
                await table.ExecuteBatchAsync(batchOperation);
                items = items.Skip(batchSize).ToList();
            }
        }

        public async Task InsertReplace(List<T> entities)
        {
            CloudTable myCloudTable = await GetTableAsync();
            var taskCount = 0;
            var taskThreshold = 300; // Seems to be a good value to start with
            var batchTasks = new List<Task<IList<TableResult>>>();
            for (var i = 0; i < entities.Count; i += TableConstants.TableServiceBatchMaximumOperations)
            {
                taskCount++;

                var batchItems = entities.Skip(i)
                                         .Take(TableConstants.TableServiceBatchMaximumOperations)
                                         .ToList();

                var batch = new TableBatchOperation();
                foreach (var item in batchItems)
                {
                    batch.InsertOrReplace(item);
                }

                var task = myCloudTable.ExecuteBatchAsync(batch);
                batchTasks.Add(task);

                if (taskCount >= taskThreshold)
                {
                    await Task.WhenAll(batchTasks);
                    taskCount = 0;
                }
            }

            await Task.WhenAll(batchTasks);
        }

        public async Task Update(T item)
        {
            CloudTable table = await GetTableAsync();

            TableOperation operation = TableOperation.InsertOrReplace(item);

            await table.ExecuteAsync(operation);
        }

        public async Task Delete(string partitionKey, string rowKey)
        {
            T item = await GetItem(partitionKey, rowKey);

            CloudTable table = await GetTableAsync();

            TableOperation operation = TableOperation.Delete(item);

            await table.ExecuteAsync(operation);
        }

        #endregion

        private readonly AzureTableSettings settings;

        private async Task<CloudTable> GetTableAsync()
        {
            CloudStorageAccount storageAccount;
            if (this.settings.StorageAccount == "devstoreaccount1")
                storageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true");
            else
                storageAccount = new CloudStorageAccount(new StorageCredentials(this.settings.StorageAccount, this.settings.StorageKey), true);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference(this.settings.TableName);
            //await table.CreateIfNotExistsAsync();

            return table;
        }
    }

    public class TableConstants
    {
        public const int TableServiceBatchMaximumOperations = 100;
    }
}
