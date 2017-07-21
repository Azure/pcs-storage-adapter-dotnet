// Copyright (c) Microsoft. All rights reserved.

using System.Runtime.CompilerServices;
using Microsoft.Azure.Documents;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Helpers;

[assembly: InternalsVisibleTo("Services.Test")]

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.Services
{
    internal sealed class KeyValueDocument : Resource
    {
        public string CollectionId { get; set; }
        public string Key { get; set; }
        public string Data { get; set; }

        public KeyValueDocument(string collectionId, string key, string data)
        {
            Id = DocumentIdHelper.GenerateId(collectionId, key);
            CollectionId = collectionId;
            Key = key;
            Data = data;
        }
    }
}
