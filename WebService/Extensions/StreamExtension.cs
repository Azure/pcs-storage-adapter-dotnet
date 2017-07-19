// Copyright (c) Microsoft. All rights reserved.

using System.IO;

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.WebService.Extensions
{
    public static class StreamExtension
    {
        public static byte[] ReadAsByteArray(this Stream source)
        {
            using (var memoryStream = new MemoryStream())
            {
                if (source.CanSeek)
                {
                    source.Seek(0, SeekOrigin.Begin);
                }

                source.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
