// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Runtime
{
    public interface IServicesConfig
    {
        string ConnectionString { get; set; }
        string ContainerName { get; set; }
    }

    public class ServicesConfig : IServicesConfig
    {
        public string ConnectionString { get; set; }
        public string ContainerName { get; set; }
    }
}
