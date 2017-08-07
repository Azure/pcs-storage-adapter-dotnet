// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
    public class DocDBKeyValueContainer : IKeyValueContainer, IDisposable
    {
        private readonly IDocumentClient client;
        private readonly IExceptionChecker exceptionChecker;
        private readonly string collectionLink;
        private readonly ILogger logger;

        public DocDBKeyValueContainer(IFactory<IDocumentClient> clientFactory,
            IExceptionChecker exceptionChecker,
            IServicesConfig config,
            ILogger logger)
        {
            this.disposedValue = false;

            this.client = clientFactory.Create();
            this.exceptionChecker = exceptionChecker;
            this.collectionLink = config.ContainerName;
            this.logger = logger;
        }

        public async Task InitializeAsync()
        {
            var regex = new Regex("dbs/(?<databaseId>.*)/colls/(?<collectionId>.*)");
            var match = regex.Match(collectionLink);
            if (!match.Success)
            {
                var message = "Invalid collection URL";
                logger.Info(message, () => new { collectionLink });
                throw new InvalidConfigurationException(message);
            }

            var databaseId = match.Groups["databaseId"].Value;
            var collectionId = match.Groups["collectionId"].Value;
            var databaseUri = UriFactory.CreateDatabaseUri(databaseId);
            var documentCollectionUri = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);

            if (await client.DocumentCollectionExistsAsync(documentCollectionUri, exceptionChecker.IsNotFoundException))
            {
                return;
            }

            if (!await client.DatabaseExistsAsync(databaseUri, exceptionChecker.IsNotFoundException))
            {
                await client.CreateDatabaseAsync(new Database { Id = databaseId });
            }

            await client.CreateDocumentCollectionAsync(
                databaseUri,
                new DocumentCollection
                {
                    Id = collectionId
                });
        }

        public async Task<ValueServiceModel> GetAsync(string collectionId, string key)
        {
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
        }

        public async Task<IEnumerable<ValueServiceModel>> GetAllAsync(string collectionId)
        {
            var query = client.CreateDocumentQuery<KeyValueDocument>(collectionLink)
                .Where(doc => doc.CollectionId == collectionId)
                .ToList();

            return await Task.FromResult(query.Select(doc => new ValueServiceModel(doc)));
        }

        public async Task<ValueServiceModel> CreateAsync(string collectionId, string key, ValueServiceModel input)
        {
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
        }

        public async Task<ValueServiceModel> UpsertAsync(string collectionId, string key, ValueServiceModel input)
        {
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
        }

        public async Task DeleteAsync(string collectionId, string key)
        {
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

        #region IDisposable Support
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    (client as IDisposable)?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
