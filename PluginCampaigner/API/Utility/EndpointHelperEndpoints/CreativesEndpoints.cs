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
    public static class CreativesEndpointHelper
    {
        private class CreativesEndpoint : Endpoint
        {
        }
        
        private class UpsertCreativesEndpoint : Endpoint
        {
            protected override string WritePathPropertyId { get; set; } = "CreativeID";
            protected override List<string> RequiredWritePropertyIds { get; set; } = new List<string>
            {
                "Name",
            };
            
            public override bool ShouldGetStaticSchema { get; set; } = true;

            public override Task<Schema> GetStaticSchemaAsync(IApiClient apiClient, Schema schema)
            {
                schema.Description = @"Notes:
The Name parameter must be unique in respect to the folder it is inserted into.
You must specify a value for either the HTML or Text parameter.
If the CreativeFolderID parameter is not specified (or 0) then the creative will be added to the root folder.";
                
                var properties = new List<Property>
                {
                    new Property
                    {
                        Id = "CreativeID",
                        Name = "CreativeID",
                        Description = "The CreativeID, if included will attempt to update the target, if not included will attempt to create a new creative.",
                        Type = PropertyType.Integer,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = true
                    },
                    new Property
                    {
                        Id = "Name",
                        Name = "Name",
                        Description = "Name of the creative.",
                        Type = PropertyType.String,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = false
                    },
                    new Property
                    {
                        Id = "HTML",
                        Name = "HTML",
                        Description = "HTML version of the creative.",
                        Type = PropertyType.String,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = false
                    },
                    new Property
                    {
                        Id = "Text",
                        Name = "Text",
                        Description = "Text version of the creative.",
                        Type = PropertyType.String,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = false
                    },
                    new Property
                    {
                        Id = "CreativeFolderID",
                        Name = "CreativeFolderID",
                        Description = "ID of the parent folder.",
                        Type = PropertyType.Integer,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = false
                    },
                    new Property
                    {
                        Id = "TrackLinks",
                        Name = "TrackLinks",
                        Description = "Boolean value indicating if the links will be parsed and tracked.",
                        Type = PropertyType.Bool,
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
        
        public static readonly Dictionary<string, Endpoint> CreativesEndpoints = new Dictionary<string, Endpoint>
        {
            {"AllCreatives", new CreativesEndpoint
            {
                Id = "AllCreatives",
                Name = "All Creatives",
                BasePath = "/Creatives",
                AllPath = "/",
                DetailPath = "/",
                DetailPropertyId = "CreativeID",
                SupportedActions = new List<EndpointActions>
                {
                    EndpointActions.Get
                },
                PropertyKeys = new List<string>
                {
                    "CreativeID"
                }
            }},
            {"UpsertCreatives", new UpsertCreativesEndpoint
            {
                Id = "UpsertCreatives",
                Name = "Upsert Creatives",
                BasePath = "/Creatives",
                AllPath = "/",
                DetailPath = "/",
                DetailPropertyId = "CreativeID",
                SupportedActions = new List<EndpointActions>
                {
                    EndpointActions.Post,
                    EndpointActions.Put
                },
                PropertyKeys = new List<string>
                {
                    "CreativeID"
                }
            }},
        };
    }
}