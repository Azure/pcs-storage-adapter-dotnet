// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Models;
using Microsoft.Azure.IoTSolutions.StorageAdapter.WebService.v1.Controllers;
using Microsoft.Azure.IoTSolutions.StorageAdapter.WebService.v1.Exceptions;
using Microsoft.Azure.IoTSolutions.StorageAdapter.WebService.Wrappers;
using Moq;
using WebService.Test.helpers;
using Xunit;

namespace WebService.Test.Controllers
{
    public class KeyValueControllerTest
    {
        private Mock<IKeyValueContainer> mockContainer;
        private Mock<IKeyGenerator> mockGenerator;
        private KeysController controller;
        private Random rand = new Random();

        public KeyValueControllerTest()
        {
            mockContainer = new Mock<IKeyValueContainer>();
            mockGenerator = new Mock<IKeyGenerator>();

            controller = new KeysController(
                mockContainer.Object,
                mockGenerator.Object,
                new Logger("UnitTest", LogLevel.Debug));
        }

        [Fact, Trait(Constants.Type, Constants.UnitTest)]
        public async Task GetTest()
        {
            var collectionId = rand.NextString();
            var key = rand.NextString();
            var data = rand.NextString();
            var etag = rand.NextString();
            var timestamp = rand.NextDateTime();

            var model = new DataServiceModel
            {
                CollectionId = collectionId,
                Key = key,
                Data = data,
                ETag = etag,
                Timestamp = timestamp
            };

            mockContainer
                .Setup(x => x.GetAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(model);

            var result = await controller.Get(collectionId, key);

            Assert.Equal(result.Key, key);
            Assert.Equal(result.Data, data);
            Assert.Equal(result.ETag, etag);
            Assert.Equal(result.metadata["$type"], "Key;1");
            Assert.Equal(result.metadata["$modified"], timestamp.ToString());
            Assert.Equal(result.metadata["$uri"], $"/v1/collections/{collectionId}/keys/{key}");

            mockContainer
                .Verify(x => x.GetAsync(
                    It.Is<string>(s => s == collectionId),
                    It.Is<string>(s => s == key)),
                    Times.Once);
        }

        [Fact, Trait(Constants.Type, Constants.UnitTest)]
        public async Task GetAllTest()
        {
            var collectionId = rand.NextString();

            var models = new DataServiceModel[]
            {
                new DataServiceModel
                {
                    CollectionId = collectionId,
                    Key = rand.NextString(),
                    Data = rand.NextString(),
                    ETag = rand.NextString(),
                    Timestamp = rand.NextDateTime()
                },
                new DataServiceModel
                {
                    CollectionId = collectionId,
                    Key = rand.NextString(),
                    Data = rand.NextString(),
                    ETag = rand.NextString(),
                    Timestamp = rand.NextDateTime()
                },
                new DataServiceModel
                {
                    CollectionId = collectionId,
                    Key = rand.NextString(),
                    Data = rand.NextString(),
                    ETag = rand.NextString(),
                    Timestamp = rand.NextDateTime()
                }
            };

            mockContainer
                .Setup(x => x.GetAllAsync(
                    It.IsAny<string>()))
                .ReturnsAsync(models);

            var result = await controller.Get(collectionId);

            Assert.Equal(result.Items.Count(), models.Length);
            foreach (var item in result.Items)
            {
                var model = models.Single(m => m.Key == item.Key);
                Assert.Equal(item.Data, model.Data);
                Assert.Equal(item.ETag, model.ETag);
                Assert.Equal(item.metadata["$type"], "Key;1");
                Assert.Equal(item.metadata["$modified"], model.Timestamp.ToString());
                Assert.Equal(item.metadata["$uri"], $"/v1/collections/{collectionId}/keys/{model.Key}");
            }
            Assert.Equal(result.metadata["$type"], "KeyList;1");
            Assert.Equal(result.metadata["$uri"], $"/v1/collections/{collectionId}/keys");

            mockContainer
                .Verify(x => x.GetAllAsync(
                    It.Is<string>(s => s == collectionId)));
        }

        [Fact, Trait(Constants.Type, Constants.UnitTest)]
        public async Task PostTest()
        {
            var collectionId = rand.NextString();
            var key = Guid.NewGuid().ToString();
            var data = rand.NextString();
            var etag = rand.NextString();
            var timestamp = rand.NextDateTime();

            var modelIn = new DataServiceModel
            {
                Data = data
            };

            var modelOut = new DataServiceModel
            {
                CollectionId = collectionId,
                Key = key,
                Data = data,
                ETag = etag,
                Timestamp = timestamp
            };

            mockGenerator
                .Setup(x => x.Generate())
                .Returns(key);

            mockContainer
                .Setup(x => x.CreateAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DataServiceModel>()))
                .ReturnsAsync(modelOut);

            var result = await controller.Post(collectionId, modelIn);

            Assert.Equal(result.Key, key);
            Assert.Equal(result.Data, data);
            Assert.Equal(result.ETag, etag);
            Assert.Equal(result.metadata["$type"], "Key;1");
            Assert.Equal(result.metadata["$modified"], modelOut.Timestamp.ToString());
            Assert.Equal(result.metadata["$uri"], $"/v1/collections/{collectionId}/keys/{key}");

            mockContainer
                .Verify(x => x.CreateAsync(
                    It.Is<string>(s => s == collectionId),
                    It.Is<string>(s => s == key),
                    It.Is<DataServiceModel>(m => m.Equals(modelIn))));
        }

        [Fact, Trait(Constants.Type, Constants.UnitTest)]
        public async Task PutNewTest()
        {
            var collectionId = rand.NextString();
            var key = rand.NextString();
            var data = rand.NextString();
            var etag = rand.NextString();
            var timestamp = rand.NextDateTime();

            var modelIn = new DataServiceModel
            {
                Data = data
            };

            var modelOut = new DataServiceModel
            {
                CollectionId = collectionId,
                Key = key,
                Data = data,
                ETag = etag,
                Timestamp = timestamp
            };

            mockContainer
                .Setup(x => x.CreateAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DataServiceModel>()))
                .ReturnsAsync(modelOut);

            var result = await controller.Put(collectionId, key, modelIn);

            Assert.Equal(result.Key, key);
            Assert.Equal(result.Data, data);
            Assert.Equal(result.ETag, etag);
            Assert.Equal(result.metadata["$type"], "Key;1");
            Assert.Equal(result.metadata["$modified"], modelOut.Timestamp.ToString());
            Assert.Equal(result.metadata["$uri"], $"/v1/collections/{collectionId}/keys/{key}");

            mockContainer
                .Verify(x => x.CreateAsync(
                    It.Is<string>(s => s == collectionId),
                    It.Is<string>(s => s == key),
                    It.Is<DataServiceModel>(m => m.Equals(modelIn))),
                    Times.Once);
        }

        [Fact, Trait(Constants.Type, Constants.UnitTest)]
        public async Task PutUpdateTest()
        {
            var collectionId = rand.NextString();
            var key = rand.NextString();
            var data = rand.NextString();
            var etagOld = rand.NextString();
            var etagNew = rand.NextString();
            var timestamp = rand.NextDateTime();

            var modelIn = new DataServiceModel
            {
                Data = data,
                ETag = etagOld
            };

            var modelOut = new DataServiceModel
            {
                CollectionId = collectionId,
                Key = key,
                Data = data,
                ETag = etagNew,
                Timestamp = timestamp
            };

            mockContainer
                .Setup(x => x.UpsertAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DataServiceModel>()))
                .ReturnsAsync(modelOut);

            var result = await controller.Put(collectionId, key, modelIn);

            Assert.Equal(result.Key, key);
            Assert.Equal(result.Data, data);
            Assert.Equal(result.ETag, etagNew);
            Assert.Equal(result.metadata["$type"], "Key;1");
            Assert.Equal(result.metadata["$modified"], modelOut.Timestamp.ToString());
            Assert.Equal(result.metadata["$uri"], $"/v1/collections/{collectionId}/keys/{key}");

            mockContainer
                .Verify(x => x.UpsertAsync(
                    It.Is<string>(s => s == collectionId),
                    It.Is<string>(s => s == key),
                    It.Is<DataServiceModel>(m => m.Equals(modelIn))),
                    Times.Once);
        }

        [Fact, Trait(Constants.Type, Constants.UnitTest)]
        public async Task DeleteTest()
        {
            var collectionId = rand.NextString();
            var key = rand.NextString();

            mockContainer
                .Setup(x => x.DeleteAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(0));

            await controller.Delete(collectionId, key);

            mockContainer
                .Verify(x => x.DeleteAsync(
                    It.Is<string>(s => s == collectionId),
                    It.Is<string>(s => s == key)),
                    Times.Once);
        }

        [Fact, Trait(Constants.Type, Constants.UnitTest)]
        public async Task ValidateKeyTest()
        {
            await Assert.ThrowsAsync<BadRequestException>(async () =>
                await controller.Delete("collection", "*"));

            await Assert.ThrowsAsync<BadRequestException>(async () =>
                await controller.Delete("collection", new string('a', 256)));
        }
    }
}
