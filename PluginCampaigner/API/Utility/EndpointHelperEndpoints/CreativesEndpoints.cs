using System.Collections.Generic;

namespace PluginCampaigner.API.Utility.EndpointHelperEndpoints
{
    public static class CreativesEndpointHelper
    {
        private class CreativesEndpoint : Endpoint
        {
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
        };
    }
}