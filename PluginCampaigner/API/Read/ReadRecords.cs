using System;
using System.Collections.Generic;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using PluginCampaigner.API.Factory;
using PluginCampaigner.API.Utility;
using PluginCampaigner.Helper;

namespace PluginCampaigner.API.Read
{
    public static partial class Read
    {
        public static async IAsyncEnumerable<Record> ReadRecordsAsync(IApiClient apiClient, Schema schema, DateTime? lastReadTime = null)
        {
            var endpointMetaJson = JsonConvert.DeserializeObject<dynamic>(schema.PublisherMetaJson);
            string endpointId = endpointMetaJson.Id;
            var endpoint = EndpointHelper.GetEndpointForId(endpointId);

            var records = endpoint?.ReadRecordsAsync(apiClient, lastReadTime);

            if (records != null)
            {
                await foreach (var record in records)
                {
                    yield return record;
                }
            }
        }
    }
}