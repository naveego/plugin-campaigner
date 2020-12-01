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
    public static class OrdersEndpointHelper
    {
        private class OrderItem
        {
            public string ProductName { get; set; }
            public string SKU { get; set; }
            public string Quantity { get; set; }
            public string UnitPrice { get; set; }
            public string Weight { get; set; }
            public string Status { get; set; }
        }
        
        private class OrdersEndpoint : Endpoint
        {
        }
        
        private class UpsertOrdersEndpoint : Endpoint
        {
            protected override string WritePathPropertyId { get; set; } = "OrderNumber";
            protected override List<string> RequiredWritePropertyIds { get; set; } = new List<string>
            {
                "OrderNumber",
                "EmailAddress",
            };
            
            public override bool ShouldGetStaticSchema { get; set; } = true;

            public override Task<Schema> GetStaticSchemaAsync(IApiClient apiClient, Schema schema)
            {
                schema.Description = @"";
                
                var properties = new List<Property>
                {
                    new Property
                    {
                        Id = "OrderNumber",
                        Name = "OrderNumber",
                        Description = "The OrderNumber, if included will attempt to update the target, if not included will attempt to create a new order.",
                        Type = PropertyType.Integer,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = true
                    },
                    new Property
                    {
                        Id = "EmailAddress",
                        Name = "EmailAddress",
                        Description = "Email address of purchaser * Required.",
                        Type = PropertyType.String,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = false
                    },
                    new Property
                    {
                        Id = "PurchaseDate",
                        Name = "PurchaseDate",
                        Description = "The date order was placed (UTC). If this property is not provided it will be set to \"Today\".",
                        Type = PropertyType.Datetime,
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
        }
        
        private class UpsertOrderItemsEndpoint : Endpoint
        {
            protected override string WritePathPropertyId { get; set; } = "OrderItemID";
            protected override List<string> RequiredWritePropertyIds { get; set; } = new List<string>
            {
                "EmailAddress",
            };
            
            public override bool ShouldGetStaticSchema { get; set; } = true;

            public override Task<Schema> GetStaticSchemaAsync(IApiClient apiClient, Schema schema)
            {
                schema.Description = @"";
                
                var properties = new List<Property>
                {
                    new Property
                    {
                        Id = "OrderItemID",
                        Name = "OrderItemID",
                        Description = "The OrderItemID, if included will attempt to update the target, if not included will attempt to create a new order item.",
                        Type = PropertyType.Integer,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = true
                    },
                    new Property
                    {
                        Id = "EmailAddress",
                        Name = "EmailAddress",
                        Description = "Email address of purchaser * Required.",
                        Type = PropertyType.String,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = false
                    },
                    new Property
                    {
                        Id = "PurchaseDate",
                        Name = "PurchaseDate",
                        Description = "The date order was placed (UTC). If this property is not provided it will be set to \"Today\".",
                        Type = PropertyType.Datetime,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = false
                    },
                    new Property
                    {
                        Id = "ProductName",
                        Name = "ProductName",
                        Description = "Name of the product.",
                        Type = PropertyType.String,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = false
                    },
                    new Property
                    {
                        Id = "SKU",
                        Name = "SKU",
                        Description = "Product SKU.",
                        Type = PropertyType.String,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = false
                    },
                    new Property
                    {
                        Id = "Quantity",
                        Name = "Quantity",
                        Description = "Number of items purchased (Defaults to 1).",
                        Type = PropertyType.Integer,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = false
                    },
                    new Property
                    {
                        Id = "UnitPrice",
                        Name = "UnitPrice",
                        Description = "Amount (per unit) paid (Defaults to 0).",
                        Type = PropertyType.Float,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = false
                    },
                    new Property
                    {
                        Id = "Weight",
                        Name = "Weight",
                        Description = "Total weight of the OrderItem (Defaults to 0).",
                        Type = PropertyType.Float,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = false
                    },
                    new Property
                    {
                        Id = "Status",
                        Name = "Status",
                        Description = "Status of the OrderItem.",
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

                if (value != null)
                {
                    postObject.Add(property.Id, value);
                }
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
                    await apiClient.PostAsync($"/Orders", json);
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
        
        public static readonly Dictionary<string, Endpoint> OrdersEndpoints = new Dictionary<string, Endpoint>
        {
            {"AllOrders", new OrdersEndpoint
            {
                Id = "AllOrders",
                Name = "All Orders",
                BasePath = "/Orders",
                AllPath = "/",
                DetailPath = "/",
                DetailPropertyId = "OrderNumber",
                SupportedActions = new List<EndpointActions>
                {
                    EndpointActions.Get
                },
                PropertyKeys = new List<string>
                {
                    "OrderNumber"
                }
            }},
            {"UpsertOrders", new OrdersEndpoint
            {
                Id = "UpsertOrders",
                Name = "Upsert Orders",
                BasePath = "/Orders",
                AllPath = "/",
                DetailPath = "/",
                DetailPropertyId = "OrderNumber",
                SupportedActions = new List<EndpointActions>
                {
                    EndpointActions.Post,
                    EndpointActions.Put
                },
                PropertyKeys = new List<string>
                {
                    "OrderNumber"
                }
            }},
            {"AllOrderItems", new OrdersEndpoint
            {
                Id = "AllOrderItems",
                Name = "All Order Items",
                BasePath = "/Orders/Items",
                AllPath = "/",
                DetailPath = "/",
                DetailPropertyId = "OrderItemID",
                SupportedActions = new List<EndpointActions>
                {
                    EndpointActions.Get
                },
                PropertyKeys = new List<string>
                {
                    "OrderItemID"
                }
            }},
            {"UpsertOrderItems", new OrdersEndpoint
            {
                Id = "UpsertOrderItems",
                Name = "Upsert Order Items",
                BasePath = "/Orders/Items",
                AllPath = "/",
                DetailPath = "/",
                DetailPropertyId = "OrderItemID",
                SupportedActions = new List<EndpointActions>
                {
                    EndpointActions.Post,
                    EndpointActions.Put
                },
                PropertyKeys = new List<string>
                {
                    "OrderItemID"
                }
            }},
        };
    }
}