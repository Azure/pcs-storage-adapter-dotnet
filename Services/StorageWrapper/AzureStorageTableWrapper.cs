// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Runtime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.Services.StorageWrapper
{
    /// <summary>
    /// Wrapper of actual Azure Storage Table, for unit test purpose
    /// </summary>
    public class AzureStorageTableWrapper : IAzureStorageTableWrapper
    {
        /// <summary>
        /// Underlying Azure Storage Table
        /// </summary>
        private CloudTable table;

        public AzureStorageTableWrapper(IServicesConfig config)
        {
            var account = CloudStorageAccount.Parse(config.StorageConnectionString);
            var client = account.CreateCloudTableClient();
            table = client.GetTableReference(config.TableName);
            table.CreateIfNotExistsAsync().Wait();
        }

        public async Task<TableResult> ExecuteAsync(TableOperation operation)
        {
            return await table.ExecuteAsync(operation);
        }

        public async Task<TableQuerySegmentWrapper<T>> ExecuteQuerySegmentedAsync<T>(TableQuery<T> query, TableContinuationToken token) where T : ITableEntity, new()
        {
            return new TableQuerySegmentWrapper<T>(await table.ExecuteQuerySegmentedAsync(query, token));
        }
    }
}
