using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
            public List<Dictionary<string, object>>? Lists { get; set; }
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


                if (recordsList.Lists == null)
                {
                    yield break;
                }
                
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
            private static ConcurrentDictionary<string, long> NameToListIdDictionary =
                new ConcurrentDictionary<string, long>();

            protected override string WritePathPropertyId { get; set; } = "";

            protected override List<string> RequiredWritePropertyIds { get; set; } = new List<string>
            {
                "Name",
                "Email"
            };

            public override bool ShouldGetStaticSchema { get; set; } = true;

            public override Task<Schema> GetStaticSchemaAsync(IApiClient apiClient, Schema schema)
            {
                schema.Description =
                    @"Creates a new list if the provided name does not yet exist and adds emails to the named list.";

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
                        Description = "Email to add to the list.",
                        Type = PropertyType.String,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = false
                    },
                    new Property
                    {
                        Id = "Email",
                        Name = "Email",
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

                // only preload conversion dictionary if empty
                if (NameToListIdDictionary.IsEmpty)
                {
                    await PreLoadLookup(apiClient);
                }

                // attempt to add email
                var errorString = await AddEmailToList(apiClient, schema, recordMap);

                if (!string.IsNullOrWhiteSpace(errorString))
                {
                    errorString = await AddNewList(apiClient, schema, recordMap);

                    // add email to new list if created
                    if (string.IsNullOrWhiteSpace(errorString))
                    {
                        errorString = await AddEmailToList(apiClient, schema, recordMap);
                    }
                }

                if (!string.IsNullOrWhiteSpace(errorString))
                {
                    var errorMessage = errorString;
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

            private async Task PreLoadLookup(IApiClient apiClient)
            {
                var readResponse = await apiClient.GetAsync(
                    $"{BasePath.TrimEnd('/')}/{AllPath.TrimStart('/')}");

                var recordsList =
                    JsonConvert.DeserializeObject<ListsResponse>(await readResponse.Content.ReadAsStringAsync());

                foreach (var list in recordsList.Lists)
                {
                    NameToListIdDictionary.TryAdd(list["Name"].ToString() ?? "UNKNOWN_LIST", (long) list["ListID"]);
                }
            }

            private async Task<string> AddEmailToList(IApiClient apiClient, Schema schema,
                Dictionary<string, object> recordMap)
            {
                if (NameToListIdDictionary.TryGetValue((string) recordMap["Name"], out var listId))
                {
                    var postObject = new Dictionary<string, object>();

                    var emailList = new List<string>
                    {
                        (string) recordMap["Email"]
                    };

                    postObject.Add("EmailAddresses", emailList);

                    var json = new StringContent(
                        JsonConvert.SerializeObject(postObject),
                        Encoding.UTF8,
                        "application/json"
                    );

                    var response =
                        await apiClient.PostAsync($"{BasePath.TrimEnd('/')}/{listId}/AddEmails", json);

                    if (!response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }

                    return "";
                }

                return "List does not exist in lookup.";
            }

            private async Task<string> AddNewList(IApiClient apiClient, Schema schema,
                Dictionary<string, object> recordMap)
            {
                var postObject = new Dictionary<string, object>();

                foreach (var property in schema.Properties)
                {
                    object value = null;

                    if (property.Id == "Email")
                    {
                        continue;
                    }

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
                    return await response.Content.ReadAsStringAsync();
                }

                var newList =
                    JsonConvert.DeserializeObject<Dictionary<string, object>>(
                        await response.Content.ReadAsStringAsync());

                if (!NameToListIdDictionary.TryAdd((string) newList["Name"], (long) newList["ListID"]))
                {
                    return "Could not add new list to lookup.";
                }

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
                "AddLists", new AddNewListsEndpoint
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