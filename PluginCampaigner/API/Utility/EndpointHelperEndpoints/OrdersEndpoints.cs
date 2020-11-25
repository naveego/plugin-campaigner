using System.Collections.Generic;

namespace PluginCampaigner.API.Utility.EndpointHelperEndpoints
{
    public static class OrdersEndpointHelper
    {
        private class OrdersEndpoint : Endpoint
        {
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
        };
    }
}