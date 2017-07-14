// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Runtime
{
    public interface IServicesConfig
    {
        string StorageConnectionString { get; set; }
        string TableName { get; set; }
    }

    public class ServicesConfig : IServicesConfig
    {
        public string StorageConnectionString { get; set; }
        public string TableName { get; set; }
    }
}
