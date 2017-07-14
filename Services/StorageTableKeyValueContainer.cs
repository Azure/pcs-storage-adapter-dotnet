// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.StorageWrapper;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.Services
{
    /// <summary>
    /// Key-value pair storage based on Azure Storage Table
    /// </summary>
    public class StorageTableKeyValueContainer : IKeyValueContainer
    {
        private ILogger logger;
        private IAzureStorageTableWrapper table;

        public StorageTableKeyValueContainer(
            IAzureStorageTableWrapper table,
            ILogger logger)
        {
            this.logger = logger;
            this.table = table;
        }

        public async Task<IEnumerable<KeyValuePair<string, object>>> SetAsync(IEnumerable<KeyValuePair<string, object>> pairs)
        {
            // Map key-value pair to insertOrReplace operation, then return the key for succeed (or null if failed)
            var tasks = pairs.Select(pair =>
                table.ExecuteAsync(TableOperation.InsertOrReplace(new ContainerTableEntity(pair.Key, pair.Value)))
                    .ContinueWith(t => t.IsFaulted ? null : pair.Key));

            var completedKeys = await WaitForAll(tasks);

            // Return add/updated key-value pairs
            return pairs.Where(pair => completedKeys.Contains(pair.Key));
        }

        public async Task<IEnumerable<KeyValuePair<string, object>>> GetAsync(IEnumerable<string> keys)
        {
            var entities = await GetEntities(keys);

            return entities.Select(entity => BuildOutputPair(entity));
        }

        public async Task<IEnumerable<KeyValuePair<string, object>>> DeleteAsync(IEnumerable<string> keys)
        {
            // Retrieve entities to be deleted
            var entities = await GetEntities(keys);
            foreach (var entity in entities)
            {
                // Use "*" as ETag
                entity.ETag = "*";
            }

            // Map entity to delete operation, then return the key for succeed (or null if failed)
            var tasks = entities.Select(entity =>
                table.ExecuteAsync(TableOperation.Delete(entity))
                    .ContinueWith(t => t.IsFaulted ? null : entity.Key));

            var completedKeys = await WaitForAll(tasks);

            // Return deleted key-value pairs
            return entities.Where(entity => completedKeys.Contains(entity.Key))
                .Select(entity => BuildOutputPair(entity));
        }

        private async Task<IEnumerable<ContainerTableEntity>> GetEntities(IEnumerable<string> keys)
        {
            if (!keys.Any())
            {
                return new ContainerTableEntity[] { };
            }

            // Build the query string
            var conditions = keys.Select(key => $"(PartitionKey {QueryComparisons.Equal} '{key}')");

            var query = new TableQuery<ContainerTableEntity>
            {
                FilterString = string.Join($" {TableOperators.Or} ", conditions)
            };

            // Retrieve segmented results
            var outputs = new List<ContainerTableEntity>();
            TableContinuationToken token = null;

            do
            {
                var result = await table.ExecuteQuerySegmentedAsync(query, token);
                token = result.ContinuationToken;

                outputs.AddRange(result.Results);
            }
            while (token != null);

            return outputs;
        }

        private async Task<IEnumerable<string>> WaitForAll(IEnumerable<Task<string>> tasks)
        {
            var results = await Task.WhenAll(tasks);
            return results.Where(key => !string.IsNullOrWhiteSpace(key));
        }

        private KeyValuePair<string, object> BuildOutputPair(ContainerTableEntity entity)
        {
            return new KeyValuePair<string, object>(entity.Key, TableColumnSerializer.Deserialize(entity.SerializedValue));
        }
    }
}
