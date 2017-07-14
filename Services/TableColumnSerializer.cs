// Copyright (c) Microsoft. All rights reserved.

using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.Services
{
    public static class TableColumnSerializer
    {
        private static JsonSerializerSettings serializeSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include };

        public static string Serialize(object value)
        {
            return JsonConvert.SerializeObject(value, Formatting.Indented, serializeSetting);
        }

        public static object Deserialize(string text)
        {
            return JsonConvert.DeserializeObject(text, serializeSetting);
        }
    }
}
