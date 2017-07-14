// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.StorageWrapper;
using Microsoft.WindowsAzure.Storage.Table;
using Xunit;

namespace Services.Test.helpers
{
    /// <summary>
    /// Mocked Azure Storage Table
    /// </summary>
    public class MockAzureStorageTableWrapper : IAzureStorageTableWrapper
    {
        private readonly Dictionary<string, ITableEntity> table = new Dictionary<string, ITableEntity>();

        /// <summary>
        /// Buggy keys to mock entities failed to be inserted, updated or deleted
        /// </summary>
        public List<string> BuggyKeys { get; private set; }

        public MockAzureStorageTableWrapper()
        {
            BuggyKeys = new List<string>();
        }

        public async Task<TableResult> ExecuteAsync(TableOperation operation)
        {
            await Task.FromResult(0);

            if (BuggyKeys.Contains(operation.Entity.PartitionKey))
            {
                throw new MockAzureStorageTableWrapperException();
            }

            switch (operation.OperationType)
            {
                case TableOperationType.InsertOrReplace:
                    table[operation.Entity.PartitionKey] = operation.Entity;
                    return new TableResult();

                case TableOperationType.Delete:
                    table.Remove(operation.Entity.PartitionKey);
                    return new TableResult();

                default:
                    throw new NotSupportedException();
            }
        }

        public async Task<TableQuerySegmentWrapper<T>> ExecuteQuerySegmentedAsync<T>(TableQuery<T> query, TableContinuationToken token) where T : ITableEntity, new()
        {
            // Query string will be parsed by regular expression
            // Currently, only one type of query string was supported: "(PartitionKey eq '<key>') [or (PartitionKey eq '<key>')]*"
            Regex queryRegex = new Regex(@"\(PartitionKey\s+eq\s+'(?<key>[^']+)'\)(\s+or\s+)?");

            // Extract the key and build result list
            var results = new List<T>();
            foreach (Match match in queryRegex.Matches(query.FilterString))
            {
                var key = match.Groups["key"].Value;

                ITableEntity entity;
                if (!table.TryGetValue(key, out entity) || !(entity is T))
                {
                    continue;
                }

                results.Add((T)entity);
            }

            // Return null as the continuous token mean no more segments
            return await Task.FromResult(new TableQuerySegmentWrapper<T>(results, null));
        }

        /// <summary>
        /// Verify table status by comparing items with desired item list
        /// </summary>
        /// <param name="desired">Desired items</param>
        public void Check(Dictionary<string, object> desired)
        {
            // Step 1: verify item count
            Assert.Equal(table.Count(), desired.Count());

            // Step 2: verify items one by one
            foreach (var pair in table)
            {
                Assert.IsType<ContainerTableEntity>(pair.Value);

                var entity = pair.Value as ContainerTableEntity;
                Assert.Equal(TableColumnSerializer.Serialize(desired[pair.Key]), entity.SerializedValue);
            }
        }
    }

    /// <summary>
    /// Mock exception
    /// </summary>
    public class MockAzureStorageTableWrapperException : Exception
    {
    }
}
