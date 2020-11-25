using System.Collections.Generic;

namespace PluginCampaigner.API.Utility.EndpointHelperEndpoints
{
    public static class ProductsEndpointHelper
    {
        private class ProductsEndpoint : Endpoint
        {
        }
        
        public static readonly Dictionary<string, Endpoint> ProductsEndpoints = new Dictionary<string, Endpoint>
        {
            {"AllProducts", new ProductsEndpoint
            {
                Id = "AllProducts",
                Name = "All Products",
                BasePath = "/Products",
                AllPath = "/",
                DetailPath = "/",
                DetailPropertyId = "ProductID",
                SupportedActions = new List<EndpointActions>
                {
                    EndpointActions.Get
                },
                PropertyKeys = new List<string>
                {
                    "ProductID"
                }
            }},
            {"AllProductCategories", new ProductsEndpoint
            {
                Id = "AllProductCategories",
                Name = "All Product Categories",
                BasePath = "/ProductCategories",
                AllPath = "/",
                DetailPath = "/",
                DetailPropertyId = "CategoryID",
                SupportedActions = new List<EndpointActions>
                {
                    EndpointActions.Get
                },
                PropertyKeys = new List<string>
                {
                    "CategoryID"
                }
            }},
        };
    }
}