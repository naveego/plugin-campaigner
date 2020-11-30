using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using PluginCampaigner.API.Factory;
using PluginCampaigner.DataContracts;

namespace PluginCampaigner.API.Utility.EndpointHelperEndpoints
{
    public static class CampaignEndpointHelper
    {
        private class CampaignEndpoint : Endpoint
        {
        }
        
        private class UpsertCampaignEndpoint : Endpoint
        {
            private static string WritePathPropertyId = "CampaignID";
            private List<string> RequiredWritePropertyIds = new List<string>
            {
                "Name",
                "CreativeID",
                "Subject",
                "FromName"
            };
            
            public override bool ShouldGetStaticSchema { get; set; } = true;

            public override Task<Schema> GetStaticSchemaAsync(IApiClient apiClient, Schema schema)
            {
                var properties = new List<Property>
                {
                    new Property
                    {
                        Id = "CampaignID",
                        Name = "CampaignID",
                        Type = PropertyType.Integer,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = false
                    },
                    new Property
                    {
                        Id = "Name",
                        Name = "Name",
                        Type = PropertyType.String,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = false
                    },
                    new Property
                    {
                        Id = "CreativeID",
                        Name = "CreativeID",
                        Type = PropertyType.Integer,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = false
                    },
                    new Property
                    {
                        Id = "Subject",
                        Name = "Subject",
                        Type = PropertyType.String,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = false
                    },
                    new Property
                    {
                        Id = "FromName",
                        Name = "FromName",
                        Type = PropertyType.String,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = false
                    },
                    new Property
                    {
                        Id = "FromEmail",
                        Name = "FromEmail",
                        Type = PropertyType.String,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = true
                    },
                    new Property
                    {
                        Id = "ToName",
                        Name = "ToName",
                        Type = PropertyType.String,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = true
                    },
                    new Property
                    {
                        Id = "PublicationID",
                        Name = "PublicationID",
                        Type = PropertyType.Integer,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = true
                    },
                    new Property
                    {
                        Id = "FilterID",
                        Name = "FilterID",
                        Type = PropertyType.Integer,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = true
                    },
                    new Property
                    {
                        Id = "ListID",
                        Name = "ListID",
                        Type = PropertyType.Integer,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = true
                    },
                    new Property
                    {
                        Id = "SourceID",
                        Name = "SourceID",
                        Type = PropertyType.Integer,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = true
                    },
                    new Property
                    {
                        Id = "UseGoogleAnalytics",
                        Name = "UseGoogleAnalytics",
                        Type = PropertyType.Bool,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = true
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
        }

        public static readonly Dictionary<string, Endpoint> CampaignEndpoints = new Dictionary<string, Endpoint>
        {
            {"AllCampaigns", new CampaignEndpoint
            {
                Id = "AllCampaigns",
                Name = "All Campaigns",
                BasePath = "/Campaigns",
                AllPath = "/All",
                DetailPath = "/",
                DetailPropertyId = "CampaignID",
                SupportedActions = new List<EndpointActions>
                {
                    EndpointActions.Get
                },
                PropertyKeys = new List<string>
                {
                    "CampaignID"
                }
            }},
            {"UpsertCampaigns", new UpsertCampaignEndpoint
            {
                Id = "UpsertCampaigns",
                Name = "Upsert Campaigns",
                BasePath = "/Campaigns",
                AllPath = "/All",
                DetailPath = "/",
                DetailPropertyId = "CampaignID",
                SupportedActions = new List<EndpointActions>
                {
                    EndpointActions.Post,
                    EndpointActions.Put
                },
                PropertyKeys = new List<string>
                {
                    "CampaignID"
                }
            }},
        };
    }
}