using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using PluginCampaigner.API.Factory;

namespace PluginCampaigner.API.Utility.EndpointHelperEndpoints
{
    public static class ListsEndpointHelper
    {
        private class ListsResponse
        {
            public List<Dictionary<string, object>> Lists { get; set; }
        }

        private class ListsEndpoint : Endpoint
        {
            public override async IAsyncEnumerable<Record> ReadRecordsAsync(IApiClient apiClient,
                DateTime? lastReadTime = null, TaskCompletionSource<DateTime>? tcs = null)
            {
                var response = await apiClient.GetAsync(
                    $"{BasePath.TrimEnd('/')}/{AllPath.TrimStart('/')}");

                var recordsList =
                    JsonConvert.DeserializeObject<ListsResponse>(await response.Content.ReadAsStringAsync());


                foreach (var recordMap in recordsList.Lists)
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

        private class AddNewListsEndpoint : Endpoint
        {
            protected override string WritePathPropertyId { get; set; } = "";

            protected override List<string> RequiredWritePropertyIds { get; set; } = new List<string>
            {
                "Name"
            };

            public override bool ShouldGetStaticSchema { get; set; } = true;

            public override Task<Schema> GetStaticSchemaAsync(IApiClient apiClient, Schema schema)
            {
                schema.Description = @"";

                var properties = new List<Property>
                {
                    new Property
                    {
                        Id = "Name",
                        Name = "Name",
                        Description = "Unique list name (less than 100 characters). * REQUIRED",
                        Type = PropertyType.String,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = false
                    },
                    new Property
                    {
                        Id = "Description",
                        Name = "Description",
                        Description = "Description of the list (less than 200 characters).",
                        Type = PropertyType.String,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = false
                    },
                };

                schema.Properties.Clear();
                schema.Properties.AddRange(properties);

                return Task.FromResult(schema);
            }

            public override async Task<string> WriteRecordAsync(IApiClient apiClient, Schema schema, Record record,
                IServerStreamWriter<RecordAck> responseStream)
            {
                var recordMap = JsonConvert.DeserializeObject<Dictionary<string, object>>(record.DataJson);

                foreach (var requiredPropertyId in RequiredWritePropertyIds)
                {
                    if (!recordMap.ContainsKey(requiredPropertyId))
                    {
                        var errorMessage = $"Record did not contain required property {requiredPropertyId}";
                        var errorAck = new RecordAck
                        {
                            CorrelationId = record.CorrelationId,
                            Error = errorMessage
                        };
                        await responseStream.WriteAsync(errorAck);

                        return errorMessage;
                    }

                    if (recordMap.ContainsKey(requiredPropertyId) && recordMap[requiredPropertyId] == null)
                    {
                        var errorMessage = $"Required property {requiredPropertyId} was NULL";
                        var errorAck = new RecordAck
                        {
                            CorrelationId = record.CorrelationId,
                            Error = errorMessage
                        };
                        await responseStream.WriteAsync(errorAck);

                        return errorMessage;
                    }
                }

                var postObject = new Dictionary<string, object>();

                foreach (var property in schema.Properties)
                {
                    object value = null;

                    if (recordMap.ContainsKey(property.Id))
                    {
                        value = recordMap[property.Id];
                    }

                    postObject.Add(property.Id, value);
                }

                var json = new StringContent(
                    JsonConvert.SerializeObject(postObject),
                    Encoding.UTF8,
                    "application/json"
                );

                var response =
                    await apiClient.PostAsync($"{BasePath.TrimEnd('/')}", json);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    var errorAck = new RecordAck
                    {
                        CorrelationId = record.CorrelationId,
                        Error = errorMessage
                    };
                    await responseStream.WriteAsync(errorAck);

                    return errorMessage;
                }

                var ack = new RecordAck
                {
                    CorrelationId = record.CorrelationId,
                    Error = ""
                };
                await responseStream.WriteAsync(ack);

                return "";
            }
        }

        public static readonly Dictionary<string, Endpoint> ListsEndpoints = new Dictionary<string, Endpoint>
        {
            {
                "AllLists", new ListsEndpoint
                {
                    Id = "AllLists",
                    Name = "All Lists",
                    BasePath = "/Lists",
                    AllPath = "/",
                    DetailPath = "/",
                    DetailPropertyId = "ListID",
                    SupportedActions = new List<EndpointActions>
                    {
                        EndpointActions.Get
                    },
                    PropertyKeys = new List<string>
                    {
                        "ListID"
                    }
                }
            },
            {
                "AddLists", new ListsEndpoint
                {
                    Id = "AddLists",
                    Name = "Add New Lists",
                    BasePath = "/Lists",
                    AllPath = "/",
                    DetailPath = "/",
                    DetailPropertyId = "ListID",
                    SupportedActions = new List<EndpointActions>
                    {
                        EndpointActions.Post
                    },
                    PropertyKeys = new List<string>
                    {
                        "ListID"
                    }
                }
            },
        };
    }
}