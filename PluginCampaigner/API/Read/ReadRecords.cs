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
        public static async IAsyncEnumerable<Record> ReadRecordsAsync(IApiClient apiClient, Schema schema)
        {
            var endpointMetaJson = JsonConvert.DeserializeObject<Endpoint>(schema.PublisherMetaJson);
            var endpoint = EndpointHelper.GetEndpointForId(endpointMetaJson.Id);

            var records = endpoint?.ReadRecordsAsync(apiClient);

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