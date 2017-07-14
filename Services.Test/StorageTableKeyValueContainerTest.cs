// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Diagnostics;
using Services.Test.helpers;
using Xunit;

namespace Services.Test
{
    public class StorageTableKeyValueContainerTest
    {
        [Fact, Trait(Constants.Type, Constants.UnitTest)]
        public async Task SetAsyncTest()
        {
            var logger = new Logger("UnitTest", LogLevel.Debug);
            var table = new MockAzureStorageTableWrapper();
            var container = new StorageTableKeyValueContainer(table, logger);

            var input = new Dictionary<string, object>();
            await container.SetAsync(input);
            table.Check(input);

            input = new Dictionary<string, object>
            {
                { "a", new { Value = 0 } }
            };
            await container.SetAsync(input);
            table.Check(input);

            input = new Dictionary<string, object>
            {
                { "a", new { Value = 1 } },
                { "b", new { Value = 1 } }
            };
            await container.SetAsync(input);
            table.Check(input);
        }

        [Fact, Trait(Constants.Type, Constants.UnitTest)]
        public async Task GetAsyncTest()
        {
            var logger = new Logger("UnitTest", LogLevel.Debug);
            var table = new MockAzureStorageTableWrapper();
            var container = new StorageTableKeyValueContainer(table, logger);

            var input = new Dictionary<string, object>
            {
                { "a", new { Value = 0 } },
                { "b", new { Value = 1 } },
                { "c", new { Value = 2 } },
                { "d", new { Value = 3 } },
            };
            await container.SetAsync(input);

            foreach (var key in input.Keys)
            {
                var retrieved = await container.GetAsync(new string[] { key });
                Assert.Equal(
                    TableColumnSerializer.Serialize(retrieved.Single().Value),
                    TableColumnSerializer.Serialize(input[key]));
            }

            var allRetrieved = await container.GetAsync(input.Keys);
            Assert.Equal(allRetrieved.Count(), input.Count());
            foreach (var pair in allRetrieved)
            {
                Assert.Equal(
                    TableColumnSerializer.Serialize(pair.Value),
                    TableColumnSerializer.Serialize(input[pair.Key]));
            }

            var emptyRetrieved = await container.GetAsync(new string[] { });
            Assert.Equal(emptyRetrieved.Count(), 0);

            var mixedRetrieved = await container.GetAsync(new string[] { input.First().Key, "nonExistKey" });
            Assert.Equal(mixedRetrieved.Count(), 1);
            Assert.Equal(
                TableColumnSerializer.Serialize(mixedRetrieved.Single().Value),
                TableColumnSerializer.Serialize(input.First().Value));
        }

        [Fact, Trait(Constants.Type, Constants.UnitTest)]
        public async Task DeleteAsyncTest()
        {
            var logger = new Logger("UnitTest", LogLevel.Debug);
            var table = new MockAzureStorageTableWrapper();
            var container = new StorageTableKeyValueContainer(table, logger);

            var input = new Dictionary<string, object>
            {
                { "a", new { Value = 0 } },
                { "b", new { Value = 1 } },
                { "c", new { Value = 2 } },
                { "d", new { Value = 3 } },
            };
            await container.SetAsync(input);

            await container.DeleteAsync(new string[] { });
            table.Check(input);

            await container.DeleteAsync(new string[] { "nonExistKey" });
            table.Check(input);

            await container.DeleteAsync(new string[] { "a" });
            input.Remove("a");
            table.Check(input);

            await container.DeleteAsync(input.Keys);
            input.Clear();
            table.Check(input);
        }

        [Fact, Trait(Constants.Type, Constants.UnitTest)]
        public async Task ServiceDownTest()
        {
            var logger = new Logger("UnitTest", LogLevel.Debug);
            var table = new MockAzureStorageTableWrapper();
            var container = new StorageTableKeyValueContainer(table, logger);

            var input = new Dictionary<string, object>
            {
                { "a", new { Value = 0 } },
                { "b", new { Value = 1 } },
                { "c", new { Value = 2 } },
                { "d", new { Value = 3 } },
            };

            table.BuggyKeys.Add("c");
            var result = await container.SetAsync(input);

            input.Remove("c");
            Assert.Equal(
                TableColumnSerializer.Serialize(result.OrderBy(pair => pair.Key)),
                TableColumnSerializer.Serialize(input.OrderBy(pair => pair.Key)));
            table.Check(input);

            table.BuggyKeys.Add("a");
            result = await container.DeleteAsync(input.Keys);

            var left = new Dictionary<string, object>
            {
                { "a", input["a"] }
            };
            input.Remove("a");
            Assert.Equal(
                TableColumnSerializer.Serialize(result.OrderBy(pair => pair.Key)),
                TableColumnSerializer.Serialize(input.OrderBy(pair => pair.Key)));
            table.Check(left);
        }
    }
}
