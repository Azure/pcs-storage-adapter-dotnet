// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.Azure.Documents;

namespace Services.Test.helpers
{
    static class ResourceExtension
    {
        public static void SetETag(this Resource resource, string etag)
        {
            resource.SetPropertyValue("_etag", etag);
        }

        public static void SetTimestamp(this Resource resource, DateTime timestamp)
        {
            resource.SetPropertyValue("_ts", new DateTimeOffset(timestamp).ToUnixTimeSeconds());
        }
    }
}
