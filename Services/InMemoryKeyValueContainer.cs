// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Diagnostics;

/// <summary>
/// Sample code showing implementation of IKeyValueContainer
/// </summary>
namespace Microsoft.Azure.IoTSolutions.StorageAdapter.Services
{
    public class InMemoryKeyValueContainer : IKeyValueContainer
    {
        private readonly ILogger logger;

        private readonly Dictionary<string, object> container = new Dictionary<string, object>();

        public InMemoryKeyValueContainer(
            ILogger logger)
        {
            this.logger = logger;
        }

        public async Task<IEnumerable<KeyValuePair<string, object>>> SetAsync(IEnumerable<KeyValuePair<string, object>> pairs)
        {
            foreach (var pair in pairs)
            {
                container[pair.Key] = pair.Value;
            }

            return await Task.FromResult(pairs);
        }

        public async Task<IEnumerable<KeyValuePair<string, object>>> GetAsync(IEnumerable<string> keys)
        {
            return await Task.FromResult(container.Where(pair => keys.Contains(pair.Key)));
        }

        public async Task<IEnumerable<KeyValuePair<string, object>>> DeleteAsync(IEnumerable<string> keys)
        {
            var outputs = container.Where(pair => keys.Contains(pair.Key));

            foreach (var key in keys)
            {
                container.Remove(key);
            }

            return await Task.FromResult(outputs);
        }
    }
}