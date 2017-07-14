// Copyright (c) Microsoft. All rights reserved.

using System;
using System.IO;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.WebService.Runtime
{
    public interface IConfig
    {
        /// <summary>Web service listening port</summary>
        int Port { get; }

        /// <summary>Service layer configuration</summary>
        IServicesConfig ServicesConfig { get; }
    }

    /// <summary>Web service configuration</summary>
    public class Config : IConfig
    {
        private const string ApplicationKey = "StorageAdapter:";
        private const string PortKey = ApplicationKey + "webservice_port";
        private const string TableNameKey = ApplicationKey + "table_name";

        private const string StorageKey = "storage:";
        private const string StorageConnectionStringKey = StorageKey + "connection_string";

        /// <summary>Web service listening port</summary>
        public int Port { get; }

        /// <summary>Service layer configuration</summary>
        public IServicesConfig ServicesConfig { get; }

        public Config(IConfigData configData)
        {
            this.Port = configData.GetInt(PortKey);

            this.ServicesConfig = new ServicesConfig
            {
                TableName = configData.GetString(TableNameKey),
                StorageConnectionString = configData.GetString(StorageConnectionStringKey)
            };
        }

        private static string MapRelativePath(string path)
        {
            if (path.StartsWith(".")) return AppContext.BaseDirectory + Path.DirectorySeparatorChar + path;
            return path;
        }
    }
}
