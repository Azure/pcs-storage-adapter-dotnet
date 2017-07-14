// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

/// <summary>
/// Interface of Azure Storage Table, for unit test purpose
/// Reminder: only operations required for unit test were implemented
/// </summary>
namespace Microsoft.Azure.IoTSolutions.StorageAdapter.Services.StorageWrapper
{
    public interface IAzureStorageTableWrapper
    {
        /// <summary>
        /// Execute table operation. Wrapper of CloudTable.ExecuteAsync
        /// </summary>
        /// <param name="operation">Operation to be executed</param>
        /// <returns>Option result</returns>
        Task<TableResult> ExecuteAsync(TableOperation operation);

        /// <summary>
        /// Query for entities. Wrapper of CloudTable.ExecuteQuerySegmentedAsync
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="query">Query</param>
        /// <param name="token">Query continuation token</param>
        /// <returns>Wrapper of TableQuerySegment</returns>
        Task<TableQuerySegmentWrapper<T>> ExecuteQuerySegmentedAsync<T>(TableQuery<T> query, TableContinuationToken token) where T : ITableEntity, new();
    }
}
