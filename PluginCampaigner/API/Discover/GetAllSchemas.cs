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
                yield return await AddSampleAndCount(apiClient, schema, sampleSize);
            }
        }

        private static async Task<Schema> AddSampleAndCount(IApiClient apiClient, Schema schema,
            int sampleSize)
        {
            // add sample and count
            var records = Read.Read.ReadRecordsAsync(apiClient, schema).Take(sampleSize);
            schema.Sample.AddRange(await records.ToListAsync());
            schema.Count = await GetCountOfRecords(apiClient, schema);

            return schema;
        }

        private static async Task<Schema> GetSchemaForEndpoint(IApiClient apiClient, Schema schema, Endpoint? endpoint)
        {
            if (endpoint == null)
            {
                return schema;
            }
            
            if (endpoint.GetDataFlowDirection() == Schema.Types.DataFlowDirection.Write)
            {
                // TODO: ADD WRITE SUPPORT
                return schema;
            }

            var recordsListRaw = await endpoint.ReadRecordsAsync(apiClient).Take(100).ToListAsync();
            var recordsList = recordsListRaw
                .Select(r => JsonConvert.DeserializeObject<Dictionary<string, object>>(r.DataJson))
                .ToList();

            var types = GetPropertyTypesFromRecords(recordsList);
            
            var record = recordsList.First();

            var properties = new List<Property>();

            foreach (var recordKey in record.Keys)
            {
                var property = new Property
                {
                    Id = recordKey,
                    Name = recordKey,
                    Type = types[recordKey],
                    IsKey = false,
                    IsCreateCounter = false,
                    IsUpdateCounter = false,
                    TypeAtSource = "",
                    IsNullable = true
                };

                properties.Add(property);
            }

            schema.Properties.Clear();
            schema.Properties.AddRange(properties);

            return schema;
        }
    }
}