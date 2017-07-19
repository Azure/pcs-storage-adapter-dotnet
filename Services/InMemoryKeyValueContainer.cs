// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Models;

/// <summary>
/// Sample code showing implementation of IKeyValueContainer
/// </summary>
namespace Microsoft.Azure.IoTSolutions.StorageAdapter.Services
{
    public class InMemoryKeyValueContainer : IKeyValueContainer
    {
        private readonly ILogger logger;

        private readonly Dictionary<string, DataServiceModel> container = new Dictionary<string, DataServiceModel>();

        public InMemoryKeyValueContainer(
            ILogger logger)
        {
            this.logger = logger;
        }

        public async Task<DataServiceModel> GetAsync(string collectionId, string key)
        {
            var id = KeyValueDocument.GenerateId(collectionId, key);

            DataServiceModel model;
            if (!container.TryGetValue(id, out model))
            {
                throw new ResourceNotFoundException();
            }

            return await Task.FromResult(model);
        }

        public async Task<IEnumerable<DataServiceModel>> GetAllAsync(string collectionId)
        {
            return await Task.FromResult(container
                .Where(pair => pair.Key.StartsWith($"{collectionId}."))
                .Select(pair => pair.Value));
        }

        public async Task<DataServiceModel> CreateAsync(string collectionId, string key, DataServiceModel input)
        {
            var id = KeyValueDocument.GenerateId(collectionId, key);

            if (container.ContainsKey(id))
            {
                throw new ConflictingResourceException();
            }

            var model = new DataServiceModel(collectionId, key, input.Data);
            container.Add(id, model);
            return await Task.FromResult(model);
        }

        public async Task<DataServiceModel> UpsertAsync(string collectionId, string key, DataServiceModel input)
        {
            var id = KeyValueDocument.GenerateId(collectionId, key);

            DataServiceModel oldModel;
            if (!container.TryGetValue(id, out oldModel))
            {
                return await CreateAsync(collectionId, key, input);
            }

            if (input.ETag != "*" && input.ETag != oldModel.ETag)
            {
                throw new ConflictingResourceException();
            }

            var newModel = new DataServiceModel(collectionId, key, input.Data);
            container[id] = newModel;
            return await Task.FromResult(newModel);
        }

        public async Task DeleteAsync(string collectionId, string key)
        {
            var id = KeyValueDocument.GenerateId(collectionId, key);

            container.Remove(id);

            await Task.FromResult(0);
        }
    }
}