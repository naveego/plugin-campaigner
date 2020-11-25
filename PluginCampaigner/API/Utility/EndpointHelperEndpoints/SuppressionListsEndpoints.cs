using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using PluginCampaigner.API.Factory;

namespace PluginCampaigner.API.Utility.EndpointHelperEndpoints
{
    public static class SuppressionListsEndpointHelper
    {
        private class SuppressionListsResponse
        {
            public List<Dictionary<string, object>> SuppressionLists { get; set; }
        }

        private class SuppressionListsEndpoint : Endpoint
        {
            public override async IAsyncEnumerable<Record> ReadRecordsAsync(IApiClient apiClient,
                DateTime? lastReadTime = null, TaskCompletionSource<DateTime>? tcs = null)
            {
                var response = await apiClient.GetAsync(
                    $"{BasePath.TrimEnd('/')}/{AllPath.TrimStart('/')}");

                var recordsList =
                    JsonConvert.DeserializeObject<SuppressionListsResponse>(await response.Content.ReadAsStringAsync());


                foreach (var recordMap in recordsList.SuppressionLists)
                {
                    var normalizedRecordMap = new Dictionary<string, object?>();

                    foreach (var kv in recordMap)
                    {
                        if (kv.Key.Equals(EndpointHelper.LinksPropertyId))
                        {
                            continue;
                        }

                        normalizedRecordMap.TryAdd(kv.Key, kv.Value);
                    }

                    yield return new Record
                    {
                        Action = Record.Types.Action.Upsert,
                        DataJson = JsonConvert.SerializeObject(normalizedRecordMap)
                    };
                }
            }
        }

        public static readonly Dictionary<string, Endpoint> SuppressionListsEndpoints = new Dictionary<string, Endpoint>
        {
            {
                "AllSuppressionLists", new SuppressionListsEndpoint
                {
                    Id = "AllSuppressionLists",
                    Name = "All SuppressionLists",
                    BasePath = "/SuppressionLists",
                    AllPath = "/",
                    DetailPath = "/",
                    DetailPropertyId = "",
                    SupportedActions = new List<EndpointActions>
                    {
                        EndpointActions.Get
                    },
                    PropertyKeys = new List<string>
                    {
                        "SuppressionListID"
                    }
                }
            },
        };
    }
}