// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Helpers
{
    static class DocumentClientExtension
    {
        public static async Task<bool> DatabaseExistsAsync(this IDocumentClient client, Uri databaseUri, Func<Exception, bool> isNotFound)
        {
            try
            {
                await client.ReadDatabaseAsync(databaseUri);
                return true;
            }
            catch (Exception ex)
            {
                if (isNotFound(ex))
                {
                    return false;
                }

                throw;
            }
        }

        public static async Task<bool> DocumentCollectionExistsAsync(this IDocumentClient client, Uri collectionUri, Func<Exception, bool> isNotFound)
        {
            try
            {
                await client.ReadDocumentCollectionAsync(collectionUri);
                return true;
            }
            catch (Exception ex)
            {
                if (isNotFound(ex))
                {
                    return false;
                }

                throw;
            }
        }
    }
}
