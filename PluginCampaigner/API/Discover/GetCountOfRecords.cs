using System.Threading.Tasks;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using PluginCampaigner.API.Factory;
using PluginCampaigner.API.Utility;
using PluginCampaigner.Helper;

namespace PluginCampaigner.API.Discover
{
    public static partial class Discover
    {
        public static Task<Count> GetCountOfRecords(IApiClient apiClient, Schema schema)
        {
            var endpointMetaJson = JsonConvert.DeserializeObject<Endpoint>(schema.PublisherMetaJson);
            var endpoint = EndpointHelper.GetEndpointForId(endpointMetaJson.Id);

            return endpoint != null
                ? endpoint.GetCountOfRecords(apiClient)
                : Task.FromResult(new Count {Kind = Count.Types.Kind.Unavailable});
        }
    }
}