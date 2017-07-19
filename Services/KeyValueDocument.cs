// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.Documents;

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.Services
{
    public class KeyValueDocument : Resource
    {
        public string CollectionId { get; set; }
        public string Key { get; set; }
        public string Data { get; set; }

        public KeyValueDocument(string collectionId, string key, string data)
        {
            Id = GenerateId(collectionId, key);
            CollectionId = collectionId;
            Key = key;
            Data = data;
        }

        public static string GenerateId(string collectionId, string key)
        {
            return $"{collectionId}.{key}";
        }
    }
}
