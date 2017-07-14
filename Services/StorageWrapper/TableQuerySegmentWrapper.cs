// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;


namespace Microsoft.Azure.IoTSolutions.StorageAdapter.Services.StorageWrapper
{
    /// <summary>
    /// Wrapper of TableQuerySegment. It is necessary since TableQuerySegment is not instantiable
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public class TableQuerySegmentWrapper<T> where T : ITableEntity, new()
    {
        /// <summary>
        /// Same as TableQuerySegment.Result
        /// </summary>
        public List<T> Results { get; private set; }

        /// <summary>
        /// Same as TableQuerySegment.ContinuationToken
        /// </summary>
        public TableContinuationToken ContinuationToken { get; private set; }

        public TableQuerySegmentWrapper(TableQuerySegment<T> segment)
        {
            Results = segment.Results;
            ContinuationToken = segment.ContinuationToken;
        }

        public TableQuerySegmentWrapper(List<T> results, TableContinuationToken token)
        {
            Results = results;
            ContinuationToken = token;
        }
    }
}
