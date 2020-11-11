using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using PluginCampaigner.API.Factory;
using PluginCampaigner.API.Utility.EndpointHelperEndpoints;
using PluginCampaigner.DataContracts;

namespace PluginCampaigner.API.Utility
{
    public static class EndpointHelper
    {
        public static readonly string LinksPropertyId = "Links";
        public static readonly string CustomFieldsId = "CustomFields";
        private static readonly Dictionary<string, Endpoint> Endpoints = new Dictionary<string, Endpoint>();

        static EndpointHelper()
        {
            CampaignEndpointHelper.CampaignEndpoints.ToList().ForEach(x => Endpoints.Add(x.Key, x.Value));
        }

        public static Dictionary<string, Endpoint> GetAllEndpoints()
        {
            return Endpoints;
        }

        public static Endpoint? GetEndpointForId(string id)
        {
            return Endpoints.ContainsKey(id) ? Endpoints[id] : null;
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

        public virtual async IAsyncEnumerable<Record> ReadRecordsAsync(IApiClient apiClient)
        {
            long pageNumber = 1;
            long maxPageNumber;

            do 
            {
                var response = await apiClient.GetAsync($"{BasePath.TrimEnd('/')}/{AllPath.TrimStart('/')}?PageNumber={pageNumber}");

                var recordsList =
                    JsonConvert.DeserializeObject<DataWrapper>(await response.Content.ReadAsStringAsync());

                maxPageNumber = recordsList.TotalPages;

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
                                await apiClient.GetAsync($"{BasePath.TrimEnd('/')}/{DetailPath.TrimStart('/')}/{kv.Value}");

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
                                JsonConvert.DeserializeObject<List<CustomField>>(JsonConvert.SerializeObject(kv.Value));
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
            } while(pageNumber <= maxPageNumber);
        }

        public virtual Task<string> WriteRecordAsync(IApiClient apiClient, Record record,
            IServerStreamWriter<RecordAck> responseStream)
        {
            throw new System.NotImplementedException();
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