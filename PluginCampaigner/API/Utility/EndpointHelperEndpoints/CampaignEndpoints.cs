using System.Collections.Generic;
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
                }
            }},
        };
    }
}