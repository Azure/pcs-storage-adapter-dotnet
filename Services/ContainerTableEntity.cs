// Copyright (c) Microsoft. All rights reserved.

using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.Services
{
    public class ContainerTableEntity : TableEntity
    {
        public ContainerTableEntity(string key, object value)
        {
            PartitionKey = key;
            RowKey = key;
            Key = key;
            SerializedValue = TableColumnSerializer.Serialize(value);
        }

        public ContainerTableEntity()
        {
        }

        public string Key { get; set; }

        public string SerializedValue { get; set; }
    }
}
