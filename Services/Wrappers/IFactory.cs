// Copyright (c) Microsoft. All rights reserved.


namespace Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Wrappers
{
    public interface IFactory<T>
    {
        T Create();
    }
}
