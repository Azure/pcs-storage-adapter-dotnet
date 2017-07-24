// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Globalization;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.WebService.v1.Models
{
    public class ValueApiModel
    {
        [JsonProperty("Key")]
        public string Key { get; set; }

        [JsonProperty("Data")]
        public string Data { get; set; }

        [JsonProperty("ETag")]
        public string ETag { get; set; }

        [JsonProperty("$metadata")]
        public Dictionary<string, string> Metadata;

        public ValueApiModel(ValueServiceModel model)
        {
            Key = model.Key;
            Data = model.Data;
            ETag = model.ETag;

            Metadata = new Dictionary<string, string>
            {
                { "$type", $"Value;{Version.Number}" },
                { "$modified", model.Timestamp.ToString(CultureInfo.InvariantCulture) },
                { "$uri", $"/{Version.Path}/collections/{model.CollectionId}/keys/{model.Key}" }
            };
        }
    }
}
