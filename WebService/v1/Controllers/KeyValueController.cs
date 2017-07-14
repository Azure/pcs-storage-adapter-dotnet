using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.StorageAdapter.WebService.v1.Exceptions;
using Microsoft.Azure.IoTSolutions.StorageAdapter.WebService.v1.Filters;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.WebService.v1.Controllers
{
    [TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class KeyValueController
    {
        private IKeyValueContainer container;
        private ILogger logger;

        public KeyValueController(
            IKeyValueContainer container,
            ILogger logger)
        {
            this.container = container;
            this.logger = logger;
        }

        [HttpPut]
        [Route(Version.Path + "/data")]
        public async Task<object> SetItemAsync([FromQuery]string key, [FromBody]object value)
        {
            if (Split(key).Count() != 1)
            {
                logger.Error($"Invalid key: {key}", () => { });
                throw new BadRequestException($"Invalid key: {key}");
            }

            return BuildOutput(await container.SetAsync(new[]
            {
                new KeyValuePair<string, object>(key, value)
            }));
        }

        [HttpPost]
        [Route(Version.Path + "/data")]
        public async Task<object> SetItemsAsync([FromBody]JObject root)
        {
            var pairs = root.Properties()
                .Select(p => new KeyValuePair<string, object>(p.Name, p.Value));

            return BuildOutput(await container.SetAsync(pairs));
        }

        [HttpGet]
        [Route(Version.Path + "/data")]
        public async Task<object> GetItemsAsync([FromQuery(Name = "key")]string keyString)
        {
            var keys = Split(keyString);
            if (keys == null)
            {
                return new object();
            }

            return BuildOutput(await container.GetAsync(keys));
        }


        [HttpDelete]
        [Route(Version.Path + "/data")]
        public async Task<object> DeleteItemsAsync([FromQuery(Name = "key")]string keyString)
        {
            var keys = Split(keyString);
            if (keys == null)
            {
                return new object();
            }

            return BuildOutput(await container.DeleteAsync(keys));
        }

        /// <summary>
        /// Build output JSON object
        /// It is necessary to refine the output to be in form { "key": "<value>" }, rather than the lengthy form: { { "key": "<key>", "value": "<value>" } }
        /// </summary>
        /// <param name="pairs">Key-Value pairs to be output</param>
        /// <returns>JSON object</returns>
        private object BuildOutput(IEnumerable<KeyValuePair<string, object>> pairs)
        {
            var root = new JObject();
            foreach (var pair in pairs)
            {
                root[pair.Key] = pair.Value == null || (pair.Value as JToken)?.Type == JTokenType.Null
                    ? null
                    : JObject.FromObject(pair.Value);
            }

            return root;
        }

        private IEnumerable<string> Split(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var items = text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (!items.Any())
            {
                return null;
            }

            return items.Select(s => s.Trim());
        }
    }
}
