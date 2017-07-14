using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.StorageAdapter.WebService.v1.Controllers;
using Microsoft.Azure.IoTSolutions.StorageAdapter.WebService.v1.Exceptions;
using Moq;
using Newtonsoft.Json.Linq;
using WebService.Test.helpers;
using Xunit;

namespace WebService.Test.Controllers
{
    public class KeyValueControllerTest
    {
        private Mock<IKeyValueContainer> keyValueContainerMock;
        private KeyValueController keyValueController;

        public KeyValueControllerTest()
        {
            keyValueContainerMock = new Mock<IKeyValueContainer>();

            keyValueController = new KeyValueController(
                keyValueContainerMock.Object,
                new Logger("UnitTest", LogLevel.Debug));
        }

        [Fact, Trait(Constants.Type, Constants.UnitTest)]
        public async Task SetItemTest()
        {
            var input = TableColumnSerializer.Deserialize("{ 'a': { 'Value': 0 } }") as JObject;

            var intermedia = new Dictionary<string, object>
            {
                { "a", new { Value = 0 } }
            };

            keyValueContainerMock
                .Setup(x => x.SetAsync(It.IsAny<IEnumerable<KeyValuePair<string, object>>>()))
                .ReturnsAsync(intermedia);

            var result = await keyValueController.SetItemAsync("a", new { Value = 0 });

            keyValueContainerMock
                .Verify(x => x.SetAsync(It.Is<IEnumerable<KeyValuePair<string, object>>>(pairs => VerifyPairs(pairs, intermedia))), Times.Once);

            Assert.Equal(
                TableColumnSerializer.Serialize(input),
                TableColumnSerializer.Serialize(result));
        }

        [Fact, Trait(Constants.Type, Constants.UnitTest)]
        public async Task SetItemMultipleKeysTest()
        {
            await Assert.ThrowsAsync<BadRequestException>(async () =>
                await keyValueController.SetItemAsync("a,b", null));
        }

        [Fact, Trait(Constants.Type, Constants.UnitTest)]
        public async Task SetItemsTest()
        {
            var input = TableColumnSerializer.Deserialize("{ 'a': { 'Value': 0 }, 'b': null, 'c': { 'String': 'Text' } }") as JObject;

            var intermedia = new Dictionary<string, object>
            {
                { "a", new { Value = 0 } },
                { "b", null },
                { "c", new { String = "Text" } }
            };

            keyValueContainerMock
                .Setup(x => x.SetAsync(It.IsAny<IEnumerable<KeyValuePair<string, object>>>()))
                .ReturnsAsync(intermedia);

            var result = await keyValueController.SetItemsAsync(input);

            keyValueContainerMock
                .Verify(x => x.SetAsync(It.Is<IEnumerable<KeyValuePair<string, object>>>(pairs => VerifyPairs(pairs, intermedia))), Times.Once);

            Assert.Equal(
                TableColumnSerializer.Serialize(input),
                TableColumnSerializer.Serialize(result));
        }

        [Fact, Trait(Constants.Type, Constants.UnitTest)]
        public async Task GetItemsSingleKeyTest()
        {
            var intermedia = new Dictionary<string, object>
            {
                { "a", new { Value = 0} }
            };

            var output = new
            {
                a = new { Value = 0 }
            };

            keyValueContainerMock
                .Setup(x => x.GetAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(intermedia);

            var result = await keyValueController.GetItemsAsync("a");

            keyValueContainerMock
                .Verify(x => x.GetAsync(It.Is<IEnumerable<string>>(keys => keys.Single() == "a")));

            Assert.Equal(
                TableColumnSerializer.Serialize(result),
                TableColumnSerializer.Serialize(output));
        }

        [Fact, Trait(Constants.Type, Constants.UnitTest)]
        public async Task GetItemsEmptyKeyTest()
        {
            var result = (await keyValueController.GetItemsAsync(string.Empty));
            Assert.Equal(TableColumnSerializer.Serialize(result), TableColumnSerializer.Serialize(new object()));

            result = (await keyValueController.GetItemsAsync(null));
            Assert.Equal(TableColumnSerializer.Serialize(result), TableColumnSerializer.Serialize(new object()));

            result = (await keyValueController.GetItemsAsync(" , ,,"));
            Assert.Equal(TableColumnSerializer.Serialize(result), TableColumnSerializer.Serialize(new object()));
        }

        [Fact, Trait(Constants.Type, Constants.UnitTest)]
        public async Task GetItemsMultipleKeysTest()
        {
            var intermedia = new Dictionary<string, object>
            {
                { "a", new { Value = 0} },
                { "b", null },
                { "c", new { String = "Text" } }
            };

            var output = TableColumnSerializer.Deserialize("{ 'a': { 'Value': 0 }, 'b': null, 'c': { 'String': 'Text' } }") as JObject;

            keyValueContainerMock
                .Setup(x => x.GetAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(intermedia);

            var result = await keyValueController.GetItemsAsync("a,b,c");

            keyValueContainerMock
                .Verify(x => x.GetAsync(It.Is<IEnumerable<string>>(keys => string.Join(",", keys) == "a,b,c")));

            Assert.Equal(
                TableColumnSerializer.Serialize(result),
                TableColumnSerializer.Serialize(output));
        }

        [Fact, Trait(Constants.Type, Constants.UnitTest)]
        public async Task DeleteItemsTest()
        {
            var intermedia = new Dictionary<string, object>
            {
                { "a", new { Value = 0} },
                { "b", null },
                { "c", new { String = "Text" } }
            };

            var output = TableColumnSerializer.Deserialize("{ 'a': { 'Value': 0 }, 'b': null, 'c': { 'String': 'Text' } }") as JObject;

            keyValueContainerMock
                .Setup(x => x.DeleteAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(intermedia);

            var result = await keyValueController.DeleteItemsAsync("a,b,c");

            keyValueContainerMock
                .Verify(x => x.DeleteAsync(It.Is<IEnumerable<string>>(keys => string.Join(",", keys) == "a,b,c")));

            Assert.Equal(
                TableColumnSerializer.Serialize(result),
                TableColumnSerializer.Serialize(output));
        }

        private bool VerifyPairs(IEnumerable<KeyValuePair<string, object>> a, IEnumerable<KeyValuePair<string, object>> b)
        {
            return TableColumnSerializer.Serialize(a.OrderBy(pair => pair.Key)) == TableColumnSerializer.Serialize(b.OrderBy(pair => pair.Key));
        }
    }
}
