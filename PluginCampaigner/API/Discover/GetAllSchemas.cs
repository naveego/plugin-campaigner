using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using PluginCampaigner.API.Factory;
using PluginCampaigner.API.Utility;
using PluginCampaigner.Helper;

namespace PluginCampaigner.API.Discover
{
    public static partial class Discover
    {
        public static async IAsyncEnumerable<Schema> GetAllSchemas(IApiClient apiClient, Settings settings,
            int sampleSize = 5)
        {
            var allEndpoints = EndpointHelper.GetAllEndpoints();

            foreach (var endpoint in allEndpoints.Values)
            {
                // base schema to be added to
                var schema = new Schema
                {
                    Id = endpoint.Id,
                    Name = endpoint.Name,
                    Description = "",
                    PublisherMetaJson = JsonConvert.SerializeObject(endpoint),
                    DataFlowDirection = endpoint.GetDataFlowDirection()
                };

                schema = await GetSchemaForEndpoint(apiClient, schema, endpoint);

                // get sample and count
                yield return await AddSampleAndCount(apiClient, schema, sampleSize, endpoint);
            }
        }

        private static async Task<Schema> AddSampleAndCount(IApiClient apiClient, Schema schema,
            int sampleSize, Endpoint? endpoint)
        {
            if (endpoint == null)
            {
                return schema;
            }
            
            // add sample and count
            var records = Read.Read.ReadRecordsAsync(apiClient, schema).Take(sampleSize);
            schema.Sample.AddRange(await records.ToListAsync());
            schema.Count = await GetCountOfRecords(apiClient, endpoint);

            return schema;
        }

        private static async Task<Schema> GetSchemaForEndpoint(IApiClient apiClient, Schema schema, Endpoint? endpoint)
        {
            if (endpoint == null)
            {
                return schema;
            }

            if (endpoint.ShouldGetStaticSchema)
            {
                return await endpoint.GetStaticSchemaAsync(apiClient, schema);
            }
            
            var recordsListRaw = await endpoint.ReadRecordsAsync(apiClient).Take(100).ToListAsync();
            var recordsList = recordsListRaw
                .Select(r => JsonConvert.DeserializeObject<Dictionary<string, object>>(r.DataJson))
                .ToList();

            var types = GetPropertyTypesFromRecords(recordsList);
            
            var record = recordsList.FirstOrDefault();

            var properties = new List<Property>();

            if (record != null)
            {
                foreach (var recordKey in record.Keys)
                {
                    var property = new Property
                    {
                        Id = recordKey,
                        Name = recordKey,
                        Type = types[recordKey],
                        IsKey = endpoint.PropertyKeys.Contains(recordKey),
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = await endpoint.IsCustomProperty(apiClient, recordKey) ? Constants.CustomProperty : "",
                        IsNullable = true
                    };

                    properties.Add(property);
                }
            }

            schema.Properties.Clear();
            schema.Properties.AddRange(properties);

            if (schema.Properties.Count == 0)
            {
                schema.Description = Constants.EmptySchemaDescription;
            }

            return schema;
        }
    }
}