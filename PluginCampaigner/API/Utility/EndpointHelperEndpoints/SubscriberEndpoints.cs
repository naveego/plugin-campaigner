using System.Collections.Generic;
using Naveego.Sdk.Plugins;
using PluginCampaigner.API.Factory;

namespace PluginCampaigner.API.Utility.EndpointHelperEndpoints
{
    public class SubscriberEndpointHelper
    {
        private class SubscriberEndpoint : Endpoint
        {
            public override IAsyncEnumerable<Record> ReadRecordsAsync(IApiClient apiClient)
            {
                return base.ReadRecordsAsync(apiClient);
            }
        }
        
        public static readonly Dictionary<string, Endpoint> CampaignEndpoints = new Dictionary<string, Endpoint>
        {
            {"ActiveSubscribers", new SubscriberEndpoint
            {
                Id = "ActiveSubscribers",
                Name = "Active Subscribers",
                BasePath = "/Subscribers",
                AllPath = "/",
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