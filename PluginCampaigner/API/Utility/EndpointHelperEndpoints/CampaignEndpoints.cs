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
            protected override string WritePathPropertyId { get; set; } = "CampaignID";
            protected override List<string> RequiredWritePropertyIds { get; set; } = new List<string>
            {
                "Name",
                "CreativeID",
                "Subject",
                "FromName"
            };
            
            public override bool ShouldGetStaticSchema { get; set; } = true;

            public override Task<Schema> GetStaticSchemaAsync(IApiClient apiClient, Schema schema)
            {
                schema.Description =
                    "Note: You can only schedule to one FilterID, ListID or SourceID. If all three are specified then the priority is: FilterID, ListID then SourceID.";
                
                var properties = new List<Property>
                {
                    new Property
                    {
                        Id = "CampaignID",
                        Name = "CampaignID",
                        Description = "The CampaignID, if included will attempt to update the target, if not included will attempt to create a new campaign.",
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
                        Description = "Name of the campaign. (Max 100 chars) * REQUIRED",
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
                        Description = "Creative ID that should be sent with this campaign. * REQUIRED",
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
                        Description = "Subject of campaign (Max 255 chars) * REQUIRED",
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
                        Description = "From Name of campaign (Max 100 chars) * REQUIRED",
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
                        Description = "From Email of campaign (Max 150 chars)",
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
                        Description = "To Name of campaign (Max 100 chars)",
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
                        Description = "Target Publication for this campaign.",
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
                        Description = "Filter ID to target for this campaign.",
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
                        Description = "List ID to target for this campaign.",
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
                        Description = "Source ID to target for this campaign.",
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
                        Description = "A boolean value indicating whether google analytics parameters should be appended to tracked links.",
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