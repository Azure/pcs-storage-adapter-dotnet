// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Globalization;
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
        private readonly Mock<IKeyValueContainer> mockContainer;
        private readonly Mock<IKeyGenerator> mockGenerator;
        private readonly ValuesController controller;
        private readonly Random rand = new Random();

        public KeyValueControllerTest()
        {
            mockContainer = new Mock<IKeyValueContainer>();
            mockGenerator = new Mock<IKeyGenerator>();

            controller = new ValuesController(
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
            var timestamp = rand.NextDateTimeOffset();

            var model = new ValueServiceModel
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
            Assert.Equal(result.Metadata["$type"], "Value;1");
            Assert.Equal(result.Metadata["$modified"], timestamp.ToString(CultureInfo.InvariantCulture));
            Assert.Equal(result.Metadata["$uri"], $"/v1/collections/{collectionId}/keys/{key}");

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

            var models = new[]
            {
                new ValueServiceModel
                {
                    CollectionId = collectionId,
                    Key = rand.NextString(),
                    Data = rand.NextString(),
                    ETag = rand.NextString(),
                    Timestamp = rand.NextDateTimeOffset()
                },
                new ValueServiceModel
                {
                    CollectionId = collectionId,
                    Key = rand.NextString(),
                    Data = rand.NextString(),
                    ETag = rand.NextString(),
                    Timestamp = rand.NextDateTimeOffset()
                },
                new ValueServiceModel
                {
                    CollectionId = collectionId,
                    Key = rand.NextString(),
                    Data = rand.NextString(),
                    ETag = rand.NextString(),
                    Timestamp = rand.NextDateTimeOffset()
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
                Assert.Equal(item.Metadata["$type"], "Value;1");
                Assert.Equal(item.Metadata["$modified"], model.Timestamp.ToString(CultureInfo.InvariantCulture));
                Assert.Equal(item.Metadata["$uri"], $"/v1/collections/{collectionId}/keys/{model.Key}");
            }
            Assert.Equal(result.Metadata["$type"], "ValueList;1");
            Assert.Equal(result.Metadata["$uri"], $"/v1/collections/{collectionId}/keys");

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
            var timestamp = rand.NextDateTimeOffset();

            var modelIn = new ValueServiceModel
            {
                Data = data
            };

            var modelOut = new ValueServiceModel
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
                    It.IsAny<ValueServiceModel>()))
                .ReturnsAsync(modelOut);

            var result = await controller.Post(collectionId, modelIn);

            Assert.Equal(result.Key, key);
            Assert.Equal(result.Data, data);
            Assert.Equal(result.ETag, etag);
            Assert.Equal(result.Metadata["$type"], "Value;1");
            Assert.Equal(result.Metadata["$modified"], modelOut.Timestamp.ToString(CultureInfo.InvariantCulture));
            Assert.Equal(result.Metadata["$uri"], $"/v1/collections/{collectionId}/keys/{key}");

            mockContainer
                .Verify(x => x.CreateAsync(
                    It.Is<string>(s => s == collectionId),
                    It.Is<string>(s => s == key),
                    It.Is<ValueServiceModel>(m => m.Equals(modelIn))));
        }

        [Fact, Trait(Constants.Type, Constants.UnitTest)]
        public async Task PutNewTest()
        {
            var collectionId = rand.NextString();
            var key = rand.NextString();
            var data = rand.NextString();
            var etag = rand.NextString();
            var timestamp = rand.NextDateTimeOffset();

            var modelIn = new ValueServiceModel
            {
                Data = data
            };

            var modelOut = new ValueServiceModel
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
                    It.IsAny<ValueServiceModel>()))
                .ReturnsAsync(modelOut);

            var result = await controller.Put(collectionId, key, modelIn);

            Assert.Equal(result.Key, key);
            Assert.Equal(result.Data, data);
            Assert.Equal(result.ETag, etag);
            Assert.Equal(result.Metadata["$type"], "Value;1");
            Assert.Equal(result.Metadata["$modified"], modelOut.Timestamp.ToString(CultureInfo.InvariantCulture));
            Assert.Equal(result.Metadata["$uri"], $"/v1/collections/{collectionId}/keys/{key}");

            mockContainer
                .Verify(x => x.CreateAsync(
                    It.Is<string>(s => s == collectionId),
                    It.Is<string>(s => s == key),
                    It.Is<ValueServiceModel>(m => m.Equals(modelIn))),
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
            var timestamp = rand.NextDateTimeOffset();

            var modelIn = new ValueServiceModel
            {
                Data = data,
                ETag = etagOld
            };

            var modelOut = new ValueServiceModel
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
                    It.IsAny<ValueServiceModel>()))
                .ReturnsAsync(modelOut);

            var result = await controller.Put(collectionId, key, modelIn);

            Assert.Equal(result.Key, key);
            Assert.Equal(result.Data, data);
            Assert.Equal(result.ETag, etagNew);
            Assert.Equal(result.Metadata["$type"], "Value;1");
            Assert.Equal(result.Metadata["$modified"], modelOut.Timestamp.ToString(CultureInfo.InvariantCulture));
            Assert.Equal(result.Metadata["$uri"], $"/v1/collections/{collectionId}/keys/{key}");

            mockContainer
                .Verify(x => x.UpsertAsync(
                    It.Is<string>(s => s == collectionId),
                    It.Is<string>(s => s == key),
                    It.Is<ValueServiceModel>(m => m.Equals(modelIn))),
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
