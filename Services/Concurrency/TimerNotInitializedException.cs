// Copyright (c) Microsoft. All rights reserved.

using System;

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Concurrency
{
    public class TimerNotInitializedException : Exception
    {
        public TimerNotInitializedException()
            : base("Timer object not initialized. Call 'Setup()' first.")
        {
        }
    }
}
