// Copyright (c) Microsoft. All rights reserved.

using System;

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Wrappers
{
    public interface IExceptionChecker
    {
        bool IsConflictException(Exception exception);
        bool IsPreconditionFailedException(Exception exception);
        bool IsNotFoundException(Exception exception);
    }
}
