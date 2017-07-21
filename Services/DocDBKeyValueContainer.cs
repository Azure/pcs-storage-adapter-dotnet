// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Helpers;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Models;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Runtime;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Wrappers;

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.Services
{
    public class DocDBKeyValueContainer : IKeyValueContainer
    {
        private readonly IFactory<IDocumentClient> clientFactory;
        private readonly IExceptionChecker exceptionChecker;
        private readonly string collectionLink;
        private readonly ILogger logger;

        public DocDBKeyValueContainer(IFactory<IDocumentClient> clientFactory,
            IExceptionChecker exceptionChecker,
            IServicesConfig config,
            ILogger logger)
        {
            this.clientFactory = clientFactory;
            this.exceptionChecker = exceptionChecker;
            this.collectionLink = config.ContainerName;
            this.logger = logger;
        }

        public async Task<ValueServiceModel> GetAsync(string collectionId, string key)
        {
            var client = clientFactory.Create();

            try
            {
                var response = await client.ReadDocumentAsync($"{collectionLink}/docs/{DocumentIdHelper.GenerateId(collectionId, key)}");
                return new ValueServiceModel(response);
            }
            catch (Exception ex)
            {
                if (exceptionChecker.IsNotFoundException(ex))
                {
                    var message = "The resource requested doesn't exist.";
                    logger.Info(message, () => new { collectionId, key });
                    throw new ResourceNotFoundException(message);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                (client as IDisposable)?.Dispose();
            }
        }

        public async Task<IEnumerable<ValueServiceModel>> GetAllAsync(string collectionId)
        {
            var client = clientFactory.Create();

            try
            {
                var query = client.CreateDocumentQuery<KeyValueDocument>(collectionLink)
                    .Where(doc => doc.CollectionId == collectionId)
                    .ToList();

                return await Task.FromResult(query.Select(doc => new ValueServiceModel(doc)));
            }
            finally
            {
                (client as IDisposable)?.Dispose();
            }
        }

        public async Task<ValueServiceModel> CreateAsync(string collectionId, string key, ValueServiceModel input)
        {
            var client = clientFactory.Create();

            try
            {
                var response = await client.CreateDocumentAsync(
                    collectionLink,
                    new KeyValueDocument(collectionId, key, input.Data));

                return new ValueServiceModel(response);
            }
            catch (Exception ex)
            {
                if (exceptionChecker.IsConflictException(ex))
                {
                    var message = "There is already a value with the key specified.";
                    logger.Info(message, () => new { collectionId, key });
                    throw new ConflictingResourceException(message);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                (client as IDisposable)?.Dispose();
            }
        }

        public async Task<ValueServiceModel> UpsertAsync(string collectionId, string key, ValueServiceModel input)
        {
            var client = clientFactory.Create();

            try
            {
                var response = await client.UpsertDocumentAsync(
                    collectionLink,
                    new KeyValueDocument(collectionId, key, input.Data),
                    IfMatch(input.ETag));

                return new ValueServiceModel(response);
            }
            catch (Exception ex)
            {
                if (exceptionChecker.IsPreconditionFailedException(ex))
                {
                    var message = "ETag mismatch: the resource has been updated by another client.";
                    logger.Info(message, () => new { collectionId, key, input.ETag });
                    throw new ConflictingResourceException(message);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                (client as IDisposable)?.Dispose();
            }
        }

        public async Task DeleteAsync(string collectionId, string key)
        {
            var client = clientFactory.Create();

            try
            {
                await client.DeleteDocumentAsync($"{collectionLink}/docs/{DocumentIdHelper.GenerateId(collectionId, key)}");
            }
            catch (Exception ex)
            {
                if (exceptionChecker.IsNotFoundException(ex))
                {
                    // No error raised even if key does not exist
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                (client as IDisposable)?.Dispose();
            }
        }

        private RequestOptions IfMatch(string etag)
        {
            if (etag == "*")
            {
                // Match all
                return null;
            }

            return new RequestOptions
            {
                AccessCondition = new AccessCondition
                {
                    Condition = etag,
                    Type = AccessConditionType.IfMatch
                }
            };
        }
    }
}
