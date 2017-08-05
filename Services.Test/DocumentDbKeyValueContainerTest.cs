// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Models;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Runtime;
using Moq;
using Services.Test.helpers;
using Xunit;

namespace Services.Test
{
    public class DocumentDbKeyValueContainerTest
    {
        private const string MockDbId = "mockdb";
        private const string MockCollId = "mockcoll";
        private static readonly string mockCollectionLink = $"/dbs/{MockDbId}/colls/{MockCollId}";

        private readonly Mock<IDocumentClient> mockClient;
        private readonly DocumentDbKeyValueContainer container;
        private readonly Random rand = new Random();

        public DocumentDbKeyValueContainerTest()
        {
            mockClient = new Mock<IDocumentClient>();

            container = new DocumentDbKeyValueContainer(
                new MockFactory<IDocumentClient>(mockClient),
                new MockExceptionChecker(),
                new ServicesConfig
                {
                    StorageType = "documentDb",
                    DocumentDbConnString = "",
                    DocumentDbDatabase = MockDbId,
                    DocumentDbCollection = MockCollId,
                    DocumentDbRUs = 567
                },
                new Logger("UnitTest", LogLevel.Debug));
        }

        [Fact, Trait(Constants.Type, Constants.UnitTest)]
        public async Task GetAsyncTest()
        {
            var collectionId = rand.NextString();
            var key = rand.NextString();
            var data = rand.NextString();
            var etag = rand.NextString();
            var timestamp = rand.NextDateTimeOffset();

            var document = new Document();
            document.SetPropertyValue("CollectionId", collectionId);
            document.SetPropertyValue("Key", key);
            document.SetPropertyValue("Data", data);
            document.SetETag(etag);
            document.SetTimestamp(timestamp);
            var response = new ResourceResponse<Document>(document);

            mockClient
                .Setup(x => x.ReadDocumentAsync(
                    It.IsAny<string>(),
                    It.IsAny<RequestOptions>()))
                .ReturnsAsync(response);

            var result = await container.GetAsync(collectionId, key);

            Assert.Equal(result.CollectionId, collectionId);
            Assert.Equal(result.Key, key);
            Assert.Equal(result.Data, data);
            Assert.Equal(result.ETag, etag);
            Assert.Equal(result.Timestamp, timestamp);

            mockClient
                .Verify(x => x.ReadDocumentAsync(
                    It.Is<string>(s => s == $"{mockCollectionLink}/docs/{collectionId}.{key}"),
                    It.IsAny<RequestOptions>()),
                    Times.Once);
        }

        [Fact, Trait(Constants.Type, Constants.UnitTest)]
        public async Task GetAsyncNotFoundTest()
        {
            var collectionId = rand.NextString();
            var key = rand.NextString();

            mockClient
                .Setup(x => x.ReadDocumentAsync(
                    It.IsAny<string>(),
                    It.IsAny<RequestOptions>()))
                .ThrowsAsync(new ResourceNotFoundException());

            await Assert.ThrowsAsync<ResourceNotFoundException>(async () =>
                await container.GetAsync(collectionId, key));
        }

        [Fact, Trait(Constants.Type, Constants.UnitTest)]
        public async Task GetAllAsyncTest()
        {
            var collectionId = rand.NextString();
            var documents = new[]
            {
                new KeyValueDocument(collectionId, rand.NextString(), rand.NextString()),
                new KeyValueDocument(collectionId, rand.NextString(), rand.NextString()),
                new KeyValueDocument(collectionId, rand.NextString(), rand.NextString()),
            };
            foreach (var doc in documents)
            {
                doc.SetETag(rand.NextString());
                doc.SetTimestamp(rand.NextDateTimeOffset());
            }

            mockClient
                .Setup(x => x.CreateDocumentQuery<KeyValueDocument>(
                    It.IsAny<string>(),
                    It.IsAny<FeedOptions>()))
                .Returns(documents.AsQueryable().OrderBy(doc => doc.Id));

            var result = (await container.GetAllAsync(collectionId)).ToList();

            Assert.Equal(result.Count(), documents.Length);
            foreach (var model in result)
            {
                var doc = documents.Single(d => d.Key == model.Key);
                Assert.Equal(model.CollectionId, collectionId);
                Assert.Equal(model.Data, doc.Data);
                Assert.Equal(model.ETag, doc.ETag);
                Assert.Equal(model.Timestamp, doc.Timestamp);
            }

            mockClient
                .Verify(x => x.CreateDocumentQuery<KeyValueDocument>(
                    It.Is<string>(s => s == mockCollectionLink),
                    It.IsAny<FeedOptions>()),
                    Times.Once);
        }

        [Fact, Trait(Constants.Type, Constants.UnitTest)]
        public async Task CreateAsyncTest()
        {
            var collectionId = rand.NextString();
            var key = rand.NextString();
            var data = rand.NextString();
            var etag = rand.NextString();
            var timestamp = rand.NextDateTimeOffset();

            var document = new Document();
            document.SetPropertyValue("CollectionId", collectionId);
            document.SetPropertyValue("Key", key);
            document.SetPropertyValue("Data", data);
            document.SetETag(etag);
            document.SetTimestamp(timestamp);
            var response = new ResourceResponse<Document>(document);

            mockClient
                .Setup(x => x.CreateDocumentAsync(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<RequestOptions>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(response);

            var result = await container.CreateAsync(collectionId, key, new ValueServiceModel
            {
                Data = data
            });

            Assert.Equal(result.CollectionId, collectionId);
            Assert.Equal(result.Key, key);
            Assert.Equal(result.Data, data);
            Assert.Equal(result.ETag, etag);
            Assert.Equal(result.Timestamp, timestamp);

            mockClient
                .Verify(x => x.CreateDocumentAsync(
                    It.Is<string>(s => s == mockCollectionLink),
                    It.Is<KeyValueDocument>(doc => doc.Id == $"{collectionId}.{key}" && doc.CollectionId == collectionId && doc.Key == key && doc.Data == data),
                    It.IsAny<RequestOptions>(),
                    It.IsAny<bool>()),
                    Times.Once);
        }

        [Fact, Trait(Constants.Type, Constants.UnitTest)]
        public async Task CreateAsyncConflictTest()
        {
            var collectionId = rand.NextString();
            var key = rand.NextString();
            var data = rand.NextString();

            mockClient
                .Setup(x => x.CreateDocumentAsync(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<RequestOptions>(),
                    It.IsAny<bool>()))
                .ThrowsAsync(new ConflictingResourceException());

            await Assert.ThrowsAsync<ConflictingResourceException>(async () =>
               await container.CreateAsync(collectionId, key, new ValueServiceModel
               {
                   Data = data
               }));
        }

        [Fact, Trait(Constants.Type, Constants.UnitTest)]
        public async Task UpsertAsyncTest()
        {
            var collectionId = rand.NextString();
            var key = rand.NextString();
            var data = rand.NextString();
            var etagOld = rand.NextString();
            var etagNew = rand.NextString();
            var timestamp = rand.NextDateTimeOffset();

            var document = new Document();
            document.SetPropertyValue("CollectionId", collectionId);
            document.SetPropertyValue("Key", key);
            document.SetPropertyValue("Data", data);
            document.SetETag(etagNew);
            document.SetTimestamp(timestamp);
            var response = new ResourceResponse<Document>(document);

            mockClient
                .Setup(x => x.UpsertDocumentAsync(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<RequestOptions>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(response);

            var result = await container.UpsertAsync(collectionId, key, new ValueServiceModel
            {
                Data = data,
                ETag = etagOld
            });

            Assert.Equal(result.CollectionId, collectionId);
            Assert.Equal(result.Key, key);
            Assert.Equal(result.Data, data);
            Assert.Equal(result.ETag, etagNew);
            Assert.Equal(result.Timestamp, timestamp);

            mockClient
                .Verify(x => x.UpsertDocumentAsync(
                    It.Is<string>(s => s == mockCollectionLink),
                    It.Is<KeyValueDocument>(doc => doc.Id == $"{collectionId}.{key}" && doc.CollectionId == collectionId && doc.Key == key && doc.Data == data),
                    It.IsAny<RequestOptions>(),
                    It.IsAny<bool>()),
                    Times.Once);
        }

        [Fact, Trait(Constants.Type, Constants.UnitTest)]
        public async Task UpsertAsyncConflictTest()
        {
            var collectionId = rand.NextString();
            var key = rand.NextString();
            var data = rand.NextString();
            var etag = rand.NextString();

            mockClient
                .Setup(x => x.UpsertDocumentAsync(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<RequestOptions>(),
                    It.IsAny<bool>()))
                .ThrowsAsync(new ConflictingResourceException());

            await Assert.ThrowsAsync<ConflictingResourceException>(async () =>
                await container.UpsertAsync(collectionId, key, new ValueServiceModel
                {
                    Data = data,
                    ETag = etag
                }));
        }

        [Fact, Trait(Constants.Type, Constants.UnitTest)]
        public async Task DeleteAsyncTest()
        {
            var collectionId = rand.NextString();
            var key = rand.NextString();

            mockClient
                .Setup(x => x.DeleteDocumentAsync(
                    It.IsAny<string>(),
                    It.IsAny<RequestOptions>()))
                .ReturnsAsync((ResourceResponse<Document>)null);

            await container.DeleteAsync(collectionId, key);

            mockClient
                .Verify(x => x.DeleteDocumentAsync(
                    It.Is<string>(s => s == $"{mockCollectionLink}/docs/{collectionId}.{key}"),
                    It.IsAny<RequestOptions>()),
                    Times.Once);
        }

        [Fact, Trait(Constants.Type, Constants.UnitTest)]
        public async Task DeleteAsyncNotFoundTest()
        {
            var collectionId = rand.NextString();
            var key = rand.NextString();

            mockClient
                .Setup(x => x.DeleteDocumentAsync(
                    It.IsAny<string>(),
                    It.IsAny<RequestOptions>()))
                .ThrowsAsync(new ResourceNotFoundException());

            await container.DeleteAsync(collectionId, key);
        }
    }
}
