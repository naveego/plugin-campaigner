using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using PluginCampaigner.API.Factory;
using PluginCampaigner.API.Utility.EndpointHelperEndpoints;
using PluginCampaigner.DataContracts;
using PluginCampaigner.Helper;

namespace PluginCampaigner.API.Utility
{
    public static class EndpointHelper
    {
        public static readonly string LinksPropertyId = "Links";
        public static readonly string CustomFieldsId = "CustomFields";
        private static readonly Dictionary<string, Endpoint> Endpoints = new Dictionary<string, Endpoint>();

        static EndpointHelper()
        {
            BouncesEndpointHelper.BouncesEndpoints.ToList().ForEach(x => Endpoints.Add(x.Key, x.Value));
            CampaignEndpointHelper.CampaignEndpoints.ToList().ForEach(x => Endpoints.Add(x.Key, x.Value));
            CreativesEndpointHelper.CreativesEndpoints.ToList().ForEach(x => Endpoints.Add(x.Key, x.Value));
            FiltersEndpointHelper.FiltersEndpoints.ToList().ForEach(x => Endpoints.Add(x.Key, x.Value));
            FoldersEndpointHelper.FoldersEndpoints.ToList().ForEach(x => Endpoints.Add(x.Key, x.Value));
            ListsEndpointHelper.ListsEndpoints.ToList().ForEach(x => Endpoints.Add(x.Key, x.Value));
            OrdersEndpointHelper.OrdersEndpoints.ToList().ForEach(x => Endpoints.Add(x.Key, x.Value));
            ProductsEndpointHelper.ProductsEndpoints.ToList().ForEach(x => Endpoints.Add(x.Key, x.Value));
            PublicationsEndpointHelper.PublicationsEndpoints.ToList().ForEach(x => Endpoints.Add(x.Key, x.Value));
            SourcesEndpointHelper.SourcesEndpoints.ToList().ForEach(x => Endpoints.Add(x.Key, x.Value));
            SubscriberEndpointHelper.SubscriberEndpoints.ToList().ForEach(x => Endpoints.Add(x.Key, x.Value));
            SuppressionListsEndpointHelper.SuppressionListsEndpoints.ToList()
                .ForEach(x => Endpoints.Add(x.Key, x.Value));
            WorkflowsEndpointHelper.WorkflowsEndpoints.ToList().ForEach(x => Endpoints.Add(x.Key, x.Value));
        }

        public static Dictionary<string, Endpoint> GetAllEndpoints()
        {
            return Endpoints;
        }

        public static Endpoint? GetEndpointForId(string id)
        {
            return Endpoints.ContainsKey(id) ? Endpoints[id] : null;
        }

        public static Endpoint? GetEndpointForSchema(Schema schema)
        {
            var endpointMetaJson = JsonConvert.DeserializeObject<dynamic>(schema.PublisherMetaJson);
            string endpointId = endpointMetaJson.Id;
            return GetEndpointForId(endpointId);
        }
    }

    public abstract class Endpoint
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string BasePath { get; set; } = "";
        public string AllPath { get; set; } = "";
        public string? DetailPath { get; set; }
        public string? DetailPropertyId { get; set; }
        public List<string> PropertyKeys { get; set; } = new List<string>();

        public virtual bool ShouldGetStaticSchema { get; set; } = false;

        protected virtual string WritePathPropertyId { get; set; } = "";

        protected virtual List<string> RequiredWritePropertyIds { get; set; } = new List<string>
        {
        };

        public List<EndpointActions> SupportedActions { get; set; } = new List<EndpointActions>();

        public virtual async Task<Count> GetCountOfRecords(IApiClient apiClient)
        {
            var response = await apiClient.GetAsync($"{BasePath.TrimEnd('/')}/{AllPath.TrimStart('/')}");

            var recordsList = JsonConvert.DeserializeObject<DataWrapper>(await response.Content.ReadAsStringAsync());

            return new Count
            {
                Kind = Count.Types.Kind.Exact,
                Value = (int) recordsList.TotalRecords
            };
        }

        public virtual async IAsyncEnumerable<Record> ReadRecordsAsync(IApiClient apiClient,
            DateTime? lastReadTime = null, TaskCompletionSource<DateTime>? tcs = null)
        {
            long pageNumber = 1;
            long maxPageNumber;
            DateTime tcsDateTime;

            do
            {
                var response = await apiClient.GetAsync(
                    $"{BasePath.TrimEnd('/')}/{AllPath.TrimStart('/')}?PageNumber={pageNumber}{(lastReadTime.HasValue ? $"&Since={lastReadTime.Value.ToUniversalTime():O}" : "")}");

                Logger.Debug($"Date Header value: {response.Headers.Date}");
                tcsDateTime = response.Headers.Date?.UtcDateTime ?? DateTime.UtcNow;

                var recordsList =
                    JsonConvert.DeserializeObject<DataWrapper>(await response.Content.ReadAsStringAsync());

                maxPageNumber = recordsList.TotalPages;

                if (recordsList.Items != null)
                {
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
                                        $"{BasePath.TrimEnd('/')}/{DetailPath.TrimStart('/')}/{kv.Value}");

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
                }

                pageNumber++;
            } while (pageNumber <= maxPageNumber);

            if (tcs != null)
            {
                Logger.Debug($"Setting tcs with value {tcsDateTime.ToUniversalTime():O}");
                tcs.SetResult(tcsDateTime);
            }
        }

        public virtual async Task<string> WriteRecordAsync(IApiClient apiClient, Schema schema, Record record,
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

            HttpResponseMessage response;

            if (!recordMap.ContainsKey(WritePathPropertyId) || recordMap.ContainsKey(WritePathPropertyId) &&
                recordMap[WritePathPropertyId] == null)
            {
                response =
                    await apiClient.PostAsync($"{BasePath.TrimEnd('/')}", json);
            }
            else
            {
                response =
                    await apiClient.PutAsync($"{BasePath.TrimEnd('/')}/{recordMap[WritePathPropertyId]}", json);
            }

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

        public virtual Task<Schema> GetStaticSchemaAsync(IApiClient apiClient, Schema schema)
        {
            throw new System.NotImplementedException();
        }

        public virtual Task<bool> IsCustomProperty(IApiClient apiClient, string propertyId)
        {
            return Task.FromResult(false);
        }

        public Schema.Types.DataFlowDirection GetDataFlowDirection()
        {
            if (CanRead() && CanWrite())
            {
                return Schema.Types.DataFlowDirection.ReadWrite;
            }

            if (CanRead() && !CanWrite())
            {
                return Schema.Types.DataFlowDirection.Read;
            }

            if (!CanRead() && CanWrite())
            {
                return Schema.Types.DataFlowDirection.Write;
            }

            return Schema.Types.DataFlowDirection.Read;
        }


        private bool CanRead()
        {
            return SupportedActions.Contains(EndpointActions.Get);
        }

        private bool CanWrite()
        {
            return SupportedActions.Contains(EndpointActions.Post) ||
                   SupportedActions.Contains(EndpointActions.Put) ||
                   SupportedActions.Contains(EndpointActions.Delete);
        }
    }

    public enum EndpointActions
    {
        Get,
        Post,
        Put,
        Delete
    }
}