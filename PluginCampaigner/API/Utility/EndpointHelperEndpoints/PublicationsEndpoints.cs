using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using PluginCampaigner.API.Factory;

namespace PluginCampaigner.API.Utility.EndpointHelperEndpoints
{
    public static class PublicationsEndpointHelper
    {
        private class PublicationsResponse
        {
            public List<Dictionary<string, object>> Publications { get; set; }
        }

        private class PublicationsEndpoint : Endpoint
        {
            public override async IAsyncEnumerable<Record> ReadRecordsAsync(IApiClient apiClient,
                DateTime? lastReadTime = null, TaskCompletionSource<DateTime>? tcs = null)
            {
                var response = await apiClient.GetAsync(
                    $"{BasePath.TrimEnd('/')}/{AllPath.TrimStart('/')}");

                var recordsList =
                    JsonConvert.DeserializeObject<PublicationsResponse>(await response.Content.ReadAsStringAsync());


                foreach (var recordMap in recordsList.Publications)
                {
                    var normalizedRecordMap = new Dictionary<string, object?>();

                    foreach (var kv in recordMap)
                    {
                        if (
                            !string.IsNullOrWhiteSpace(DetailPath) &&
                            !string.IsNullOrWhiteSpace(DetailPropertyId) &&
                            kv.Key.Equals(DetailPropertyId) && kv.Value != null)
                        {
                            var detailResponse =
                                await apiClient.GetAsync(
                                    $"{BasePath.TrimEnd('/')}/{DetailPath.TrimStart('/')}/{kv.Value}");

                            var detailsRecord =
                                JsonConvert.DeserializeObject<Dictionary<string, object>>(
                                    await detailResponse.Content.ReadAsStringAsync());

                            foreach (var detailKv in detailsRecord)
                            {
                                if (detailKv.Key.Equals(EndpointHelper.LinksPropertyId))
                                {
                                    continue;
                                }

                                normalizedRecordMap.TryAdd(detailKv.Key, detailKv.Value);
                            }

                            continue;
                        }

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

        public static readonly Dictionary<string, Endpoint> PublicationsEndpoints = new Dictionary<string, Endpoint>
        {
            {
                "AllPublications", new PublicationsEndpoint
                {
                    Id = "AllPublications",
                    Name = "All Publications",
                    BasePath = "/Publications",
                    AllPath = "/",
                    DetailPath = "/",
                    DetailPropertyId = "PublicationID",
                    SupportedActions = new List<EndpointActions>
                    {
                        EndpointActions.Get
                    },
                    PropertyKeys = new List<string>
                    {
                        "PublicationID"
                    }
                }
            },
        };
    }
}