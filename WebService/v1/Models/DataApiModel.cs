// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.WebService.v1.Models
{
    public class DataApiModel
    {
        [JsonProperty("Key")]
        public string Key { get; set; }

        [JsonProperty("Data")]
        public string Data { get; set; }

        [JsonProperty("ETag")]
        public string ETag { get; set; }

        [JsonProperty("$metadata")]
        public Dictionary<string, string> metadata;

        public DataApiModel(DataServiceModel model)
        {
            Key = model.Key;
            Data = model.Data;
            ETag = model.ETag;

            metadata = new Dictionary<string, string>
            {
                { "$type", $"Key;{Version.Number}" },
                { "$modified", model.Timestamp.ToString() },
                { "$uri", $"/{Version.Path}/collections/{model.CollectionId}/keys/{model.Key}" }
            };
        }
    }
}
