// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Models
{
    public class DataServiceModel
    {
        public string CollectionId { get; set; }
        public string Key { get; set; }
        public string Data { get; set; }
        public string ETag { get; set; }
        public DateTime Timestamp { get; set; }

        public DataServiceModel()
        {
        }

        public DataServiceModel(ResourceResponse<Document> response)
        {
            var resource = response.Resource;

            CollectionId = resource.GetPropertyValue<string>("CollectionId");
            Key = resource.GetPropertyValue<string>("Key");
            Data = resource.GetPropertyValue<string>("Data");
            ETag = resource.ETag;
            Timestamp = resource.Timestamp;
        }

        public DataServiceModel(KeyValueDocument document)
        {
            CollectionId = document.CollectionId;
            Key = document.Key;
            Data = document.Data;
            ETag = document.ETag;
            Timestamp = document.Timestamp;
        }

        public DataServiceModel(string collectionId, string key, string data)
        {
            CollectionId = collectionId;
            Key = key;
            Data = data;
            ETag = Guid.NewGuid().ToString();
            Timestamp = DateTime.UtcNow;
        }
    }
}