// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.Services
{
    /// <summary>
    /// Common interface for underlying key-value storage services, such as Azure Storage Table, Cosmos DB and so on
    /// </summary>
    public interface IKeyValueContainer
    {
        /// <summary>
        /// Add/Update key-value pairs
        /// </summary>
        /// <param name="pairs">Key-value pairs to be added or updated</param>
        /// <returns>Pairs were successfully added or updated. Failed pairs will not be included</returns>
        Task<IEnumerable<KeyValuePair<string, object>>> SetAsync(IEnumerable<KeyValuePair<string, object>> pairs);

        /// <summary>
        /// Retrieve key-value pairs
        /// </summary>
        /// <param name="keys">Retrieving keys</param>
        /// <returns>Successfully retrieved pairs. Keys were not found will not be included</returns>
        Task<IEnumerable<KeyValuePair<string, object>>> GetAsync(IEnumerable<string> keys);

        /// <summary>
        /// Remove key-value pairs
        /// </summary>
        /// <param name="keys">Deleting keys</param>
        /// <returns>Pairs were successfully removed. Failed pairs will not be included</returns>
        Task<IEnumerable<KeyValuePair<string, object>>> DeleteAsync(IEnumerable<string> keys);
    }
}
