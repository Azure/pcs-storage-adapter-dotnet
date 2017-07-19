// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.WebService.v1.Models
{
    public class DataListApiModel
    {
        public IEnumerable<DataApiModel> Items;

        [JsonProperty("$metadata")]
        public Dictionary<string, string> metadata;

        public DataListApiModel(IEnumerable<DataServiceModel> models, string collectionId)
        {
            Items = models.Select(m => new DataApiModel(m));

            metadata = new Dictionary<string, string>
            {
                { "$type", $"KeyList;{Version.Number}" },
                { "$uri", $"/{Version.Path}/collections/{collectionId}/keys" }
            };
        }
    }
}
