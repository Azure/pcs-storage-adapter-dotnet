// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Helpers;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Models;

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.Services
{
    /// <summary>
    /// Sample code showing implementation of IKeyValueContainer
    /// </summary>
    public class InMemoryKeyValueContainer : IKeyValueContainer
    {
        private readonly ILogger logger;

        private readonly Dictionary<string, ValueServiceModel> container = new Dictionary<string, ValueServiceModel>();

        public InMemoryKeyValueContainer(
            ILogger logger)
        {
            this.logger = logger;
        }

        public async Task<ValueServiceModel> GetAsync(string collectionId, string key)
        {
            var id = DocumentIdHelper.GenerateId(collectionId, key);

            ValueServiceModel model;
            if (!container.TryGetValue(id, out model))
            {
                logger.Info("The resource requested doesn't exist.", () => new { collectionId, key });
                throw new ResourceNotFoundException();
            }

            return await Task.FromResult(model);
        }

        public async Task<IEnumerable<ValueServiceModel>> GetAllAsync(string collectionId)
        {
            return await Task.FromResult(container
                .Where(pair => pair.Key.StartsWith($"{collectionId}."))
                .Select(pair => pair.Value));
        }

        public async Task<ValueServiceModel> CreateAsync(string collectionId, string key, ValueServiceModel input)
        {
            var id = DocumentIdHelper.GenerateId(collectionId, key);

            if (container.ContainsKey(id))
            {
                logger.Info("There is already a value with the key specified.", () => new { collectionId, key });
                throw new ConflictingResourceException();
            }

            var model = new ValueServiceModel
            {
                CollectionId = collectionId,
                Key = key,
                Data = input.Data,
                ETag = Guid.NewGuid().ToString(),
                Timestamp = DateTimeOffset.UtcNow
            };
            container.Add(id, model);
            return await Task.FromResult(model);
        }

        public async Task<ValueServiceModel> UpsertAsync(string collectionId, string key, ValueServiceModel input)
        {
            var id = DocumentIdHelper.GenerateId(collectionId, key);

            ValueServiceModel oldModel;
            if (!container.TryGetValue(id, out oldModel))
            {
                return await CreateAsync(collectionId, key, input);
            }

            if (input.ETag != "*" && input.ETag != oldModel.ETag)
            {
                logger.Info("ETag mismatch: the resource has been updated by another client.", () => new { collectionId, key, input.ETag });
                throw new ConflictingResourceException();
            }

            var newModel = new ValueServiceModel
            {
                CollectionId = collectionId,
                Key = key,
                Data = input.Data,
                ETag = Guid.NewGuid().ToString(),
                Timestamp = DateTimeOffset.UtcNow
            };
            container[id] = newModel;
            return await Task.FromResult(newModel);
        }

        public async Task DeleteAsync(string collectionId, string key)
        {
            var id = DocumentIdHelper.GenerateId(collectionId, key);

            container.Remove(id);

            await Task.FromResult(0);
        }
    }
}