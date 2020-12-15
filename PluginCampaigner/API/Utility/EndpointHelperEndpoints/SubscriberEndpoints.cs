using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            private List<string> ColumnPropertyIds = new List<string>();

            private static string WritePathPropertyId = "EmailAddress";

            private List<string> RequiredWritePropertyIds = new List<string>
            {
                WritePathPropertyId,
            };


            public override async IAsyncEnumerable<Record> ReadRecordsAsync(IApiClient apiClient,
                DateTime? lastReadTime = null, TaskCompletionSource<DateTime>? tcs = null, bool isDiscoverRead = false)
            {
                long pageNumber = 1;
                long maxPageNumber;
                DateTime tcsDateTime;

                var columnPropertyIds = await GetColumnPropertyIds(apiClient);
                var columnString = string.Join(",", columnPropertyIds);

                do
                {
                    var response = await apiClient.GetAsync(
                        $"{BasePath.TrimEnd('/')}/{AllPath.TrimStart('/')}?PageNumber={pageNumber}&Fields={columnString}{(lastReadTime.HasValue ? $"&Since={lastReadTime.Value.ToUniversalTime():O}" : "")}");

                    tcsDateTime = response.Headers.Date?.UtcDateTime ?? DateTime.UtcNow;

                    var recordsList =
                        JsonConvert.DeserializeObject<DataWrapper>(await response.Content.ReadAsStringAsync());

                    maxPageNumber = recordsList.TotalPages;
                    
                    if (recordsList.Items == null)
                    {
                        yield break;
                    }
                    
                    foreach (var recordMap in recordsList.Items)
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
                                        $"{BasePath.TrimEnd('/')}/{kv.Value}/{DetailPath.TrimStart('/')}");

                                var detailsRecord =
                                    JsonConvert.DeserializeObject<Dictionary<string, object>>(
                                        await detailResponse.Content.ReadAsStringAsync());

                                foreach (var detailKv in detailsRecord)
                                {
                                    if (detailKv.Key.Equals(EndpointHelper.CustomFieldsId) && detailKv.Value != null)
                                    {
                                        var customFields =
                                            JsonConvert.DeserializeObject<List<CustomField>>(
                                                JsonConvert.SerializeObject(detailKv.Value));
                                        foreach (var cf in customFields)
                                        {
                                            normalizedRecordMap.TryAdd(cf.FieldName, cf.Value);
                                        }

                                        continue;
                                    }

                                    if (detailKv.Key.Equals(EndpointHelper.LinksPropertyId))
                                    {
                                        continue;
                                    }
                                    
                                    if (detailKv.Value is JToken && !isDiscoverRead)
                                    {
                                        var jTokenJson = JsonConvert.SerializeObject(detailKv.Value);
                                        if (jTokenJson == "[]")
                                        {
                                            normalizedRecordMap.TryAdd(detailKv.Key, null);
                                        }
                                        
                                        normalizedRecordMap.TryAdd(detailKv.Key, jTokenJson);
                                        continue;
                                    }

                                    normalizedRecordMap.TryAdd(detailKv.Key, detailKv.Value);
                                }

                                continue;
                            }
                            
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

                var putObject = new Dictionary<string, object>();
                var customFieldObject = new List<CustomField>();

                foreach (var property in schema.Properties)
                {
                    if (property.TypeAtSource == Constants.CustomProperty)
                    {
                        var customField = new CustomField
                        {
                            FieldName = property.Id,
                            Value = null
                        };

                        if (recordMap.ContainsKey(property.Id))
                        {
                            customField.Value = recordMap[property.Id];
                        }

                        customFieldObject.Add(customField);
                    }
                    else
                    {
                        object value = null;

                        if (recordMap.ContainsKey(property.Id))
                        {
                            value = recordMap[property.Id];
                            
                            if (property.Type == PropertyType.Json)
                            {
                                if (value is string s)
                                {
                                    value = JsonConvert.DeserializeObject(s) ?? null;
                                }
                            }
                        }

                        putObject.Add(property.Id, value);
                    }
                }

                putObject.Add("CustomFields", customFieldObject);

                if (putObject.ContainsKey("Lists") && putObject["Lists"] != null)
                {
                    JArray j = (JArray) putObject["Lists"];
                    putObject["Lists"] = j.Select(x => x["ListID"]).ToList();
                }
                
                if (putObject.ContainsKey("Orders") && putObject["Orders"] != null)
                {
                    JArray j = (JArray) putObject["Orders"];
                    putObject["Orders"] = j.Select(x => x["OrderNumber"]).ToList();
                }
                
                if (putObject.ContainsKey("Publications") && putObject["Publications"] != null)
                {
                    JArray j = (JArray) putObject["Publications"];
                    putObject["Publications"] = j.Select(x => x["PublicationID"]).ToList();
                }
                
                if (putObject.ContainsKey("Source") && putObject["Source"] != null)
                {
                    JObject j = (JObject) putObject["Source"];
                    putObject["SourceID"] = j["SourceID"];
                    putObject.Remove("Source");
                }
                
                var json = new StringContent(
                    JsonConvert.SerializeObject(putObject),
                    Encoding.UTF8,
                    "application/json"
                );

                var response =
                    await apiClient.PutAsync($"{BasePath.TrimEnd('/')}/{recordMap[WritePathPropertyId]}", json);
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

            public override async Task<bool> IsCustomProperty(IApiClient apiClient, string propertyId)
            {
                if (ColumnPropertyIds.Count == 0)
                {
                    ColumnPropertyIds = await GetColumnPropertyIds(apiClient);
                }

                return ColumnPropertyIds.Contains(propertyId);
            }

            private async Task<List<string>> GetColumnPropertyIds(IApiClient apiClient)
            {
                var columnResponse = await apiClient.GetAsync($"{ColumnPath.TrimEnd('/')}");
                var columnList =
                    JsonConvert.DeserializeObject<DatabaseColumnsWrapper>(
                        await columnResponse.Content.ReadAsStringAsync());

                ColumnPropertyIds = (
                        columnList.DatabaseColumns ??
                        new List<DatabaseColumn>()).Select(c => c.ColumnName
                    )
                    .ToList();
                return ColumnPropertyIds;
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
                    DetailPath = "/Properties",
                    DetailPropertyId = "EmailAddress",
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
            {
                "UpsertSubscribers", new SubscriberEndpoint
                {
                    Id = "UpsertSubscribers",
                    Name = "Upsert Subscribers",
                    BasePath = "/Subscribers",
                    AllPath = "/",
                    DetailPath = "/Properties",
                    DetailPropertyId = "EmailAddress",
                    SupportedActions = new List<EndpointActions>
                    {
                        EndpointActions.Post,
                        EndpointActions.Put,
                        EndpointActions.Delete
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