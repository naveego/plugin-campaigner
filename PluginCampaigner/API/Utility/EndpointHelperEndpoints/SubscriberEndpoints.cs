using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using PluginCampaigner.API.Factory;
using PluginCampaigner.DataContracts;
using PluginCampaigner.Helper;

namespace PluginCampaigner.API.Utility.EndpointHelperEndpoints
{
    public class SubscriberEndpointHelper
    {
        private class SubscriberEndpoint : Endpoint
        {
            private string ColumnPath = "/Database";

            public override async IAsyncEnumerable<Record> ReadRecordsAsync(IApiClient apiClient,
                DateTime? lastReadTime = null, TaskCompletionSource<DateTime>? tcs = null)
            {
                long pageNumber = 1;
                long maxPageNumber;
                DateTime tcsDateTime;

                var columnResponse = await apiClient.GetAsync($"{ColumnPath.TrimEnd('/')}");
                var columnList =
                    JsonConvert.DeserializeObject<DatabaseColumnsWrapper>(
                        await columnResponse.Content.ReadAsStringAsync());
                var columnString = string.Join(",", columnList.DatabaseColumns.Select(c => c.ColumnName).ToList());

                do
                {
                    var response = await apiClient.GetAsync(
                        $"{BasePath.TrimEnd('/')}/{AllPath.TrimStart('/')}?PageNumber={pageNumber}&Fields={columnString}{(lastReadTime.HasValue ? $"&Since={lastReadTime.Value.ToUniversalTime():O}" : "")}");

                    tcsDateTime = response.Headers.Date?.UtcDateTime ?? DateTime.UtcNow;

                    var recordsList =
                        JsonConvert.DeserializeObject<DataWrapper>(await response.Content.ReadAsStringAsync());

                    maxPageNumber = recordsList.TotalPages;

                    foreach (var recordMap in recordsList.Items)
                    {
                        var normalizedRecordMap = new Dictionary<string, object?>();

                        foreach (var kv in recordMap)
                        {
                            if (kv.Key.Equals(EndpointHelper.CustomFieldsId) && kv.Value != null)
                            {
                                var customFields =
                                    JsonConvert.DeserializeObject<List<CustomField>>(
                                        JsonConvert.SerializeObject(kv.Value));
                                foreach (var cf in customFields)
                                {
                                    normalizedRecordMap.TryAdd(cf.FieldName, cf.Value);
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

                    pageNumber++;
                } while (pageNumber <= maxPageNumber);

                if (tcs != null)
                {
                    Logger.Debug($"Setting tcs with value {tcsDateTime.ToUniversalTime():O}");
                    tcs.SetResult(tcsDateTime);
                }
            }
        }

        public static readonly Dictionary<string, Endpoint> SubscriberEndpoints = new Dictionary<string, Endpoint>
        {
            {
                "ActiveSubscribers", new SubscriberEndpoint
                {
                    Id = "ActiveSubscribers",
                    Name = "Active Subscribers",
                    BasePath = "/Subscribers",
                    AllPath = "/",
                    DetailPath = null,
                    DetailPropertyId = null,
                    SupportedActions = new List<EndpointActions>
                    {
                        EndpointActions.Get
                    },
                    PropertyKeys = new List<string>
                    {
                        "EmailID"
                    }
                }
            },
            {
                "RemovedSubscribers", new SubscriberEndpoint
                {
                    Id = "RemovedSubscribers",
                    Name = "Removed Subscribers",
                    BasePath = "/Subscribers",
                    AllPath = "/Removes",
                    DetailPath = null,
                    DetailPropertyId = null,
                    SupportedActions = new List<EndpointActions>
                    {
                        EndpointActions.Get
                    },
                    PropertyKeys = new List<string>
                    {
                        "EmailID"
                    }
                }
            },
        };
    }
}