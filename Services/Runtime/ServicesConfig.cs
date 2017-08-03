// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Runtime
{
    public interface IServicesConfig
    {
        string DocDBConnString { get; set; }
        string ContainerName { get; set; }
    }

    public class ServicesConfig : IServicesConfig
    {
        public string DocDBConnString { get; set; }
        public string ContainerName { get; set; }
    }
}
