// Copyright (c) Microsoft. All rights reserved.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Models;
using Microsoft.Azure.IoTSolutions.StorageAdapter.WebService.v1.Exceptions;
using Microsoft.Azure.IoTSolutions.StorageAdapter.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.StorageAdapter.WebService.v1.Models;
using Microsoft.Azure.IoTSolutions.StorageAdapter.WebService.Wrappers;

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.WebService.v1.Controllers
{
    [Route(Version.Path), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class KeysController : Controller
    {
        private readonly IKeyValueContainer container;
        private readonly IKeyGenerator generator;
        private readonly ILogger logger;

        public KeysController(
            IKeyValueContainer container,
            IKeyGenerator generator,
            ILogger logger)
        {
            this.container = container;
            this.generator = generator;
            this.logger = logger;
        }

        [HttpGet("collections/{collectionId}/keys/{key}")]
        public async Task<DataApiModel> Get(string collectionId, string key)
        {
            EnsureValidId(collectionId, key);

            var result = await container.GetAsync(collectionId, key);

            return new DataApiModel(result);
        }

        [HttpGet("collections/{collectionId}/keys")]
        public async Task<DataListApiModel> Get(string collectionId)
        {
            EnsureValidId(collectionId);

            var result = await container.GetAllAsync(collectionId);

            return new DataListApiModel(result, collectionId);
        }

        [HttpPost("collections/{collectionId}/keys")]
        public async Task<DataApiModel> Post(string collectionId, [FromBody]DataServiceModel model)
        {
            string key = generator.Generate();
            EnsureValidId(collectionId, key);

            var result = await container.CreateAsync(collectionId, key, model);

            return new DataApiModel(result);
        }

        [HttpPut("collections/{collectionId}/keys/{key}")]
        public async Task<DataApiModel> Put(string collectionId, string key, [FromBody]DataServiceModel model)
        {
            EnsureValidId(collectionId, key);

            var result = model.ETag == null ?
                await container.CreateAsync(collectionId, key, model) :
                await container.UpsertAsync(collectionId, key, model);

            return new DataApiModel(result);
        }

        [HttpDelete("collections/{collectionId}/keys/{key}")]
        public async Task Delete(string collectionId, string key)
        {
            EnsureValidId(collectionId, key);

            await container.DeleteAsync(collectionId, key);
        }

        private void EnsureValidId(string collectionId, string key = "")
        {
            var validCharacters = "_-.";

            string id = KeyValueDocument.GenerateId(collectionId, key);

            if (id.Length > 255 || id.Any(c => !char.IsLetterOrDigit(c) && !validCharacters.Contains(c)))
            {
                var message = $"Invalid Collection ID/Key: '{collectionId}', '{key}'";
                logger.Error(message, () => { });
                throw new BadRequestException(message);
            }
        }
    }
}
