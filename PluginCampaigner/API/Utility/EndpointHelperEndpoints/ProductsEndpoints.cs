using System.Collections.Generic;
using System.Threading.Tasks;
using Naveego.Sdk.Plugins;
using PluginCampaigner.API.Factory;

namespace PluginCampaigner.API.Utility.EndpointHelperEndpoints
{
    public static class ProductsEndpointHelper
    {
        private class ProductsEndpoint : Endpoint
        {
        }
        
        private class UpsertProductsEndpoint : Endpoint
        {
            protected override string WritePathPropertyId { get; set; } = "ProductID";
            protected override List<string> RequiredWritePropertyIds { get; set; } = new List<string>
            {
            };
            
            public override bool ShouldGetStaticSchema { get; set; } = true;

            public override Task<Schema> GetStaticSchemaAsync(IApiClient apiClient, Schema schema)
            {
                schema.Description = @"Note: A ProductName or SKU is required to add a new product.";
                
                var properties = new List<Property>
                {
                    new Property
                    {
                        Id = "ProductID",
                        Name = "ProductID",
                        Description = "The ProductID, if included will attempt to update the target, if not included will attempt to create a new product.",
                        Type = PropertyType.Integer,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = true
                    },
                    new Property
                    {
                        Id = "ProductName",
                        Name = "ProductName",
                        Description = "Name of the product (less than 200 characters).",
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
                        Description = "Unique SKU for this product (less than 200 characters) * Must Be Unique",
                        Type = PropertyType.String,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = false
                    },
                    new Property
                    {
                        Id = "LongDescription",
                        Name = "LongDescription",
                        Description = "Long Description (less than 2,000 characters).",
                        Type = PropertyType.String,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = false
                    },
                    new Property
                    {
                        Id = "ShortDescription",
                        Name = "ShortDescription",
                        Description = "Short Description (less than 200 characters).",
                        Type = PropertyType.String,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = false
                    },
                    new Property
                    {
                        Id = "ProductURL",
                        Name = "ProductURL",
                        Description = "URL for this product (less than 200 characters).",
                        Type = PropertyType.String,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = false
                    },
                    new Property
                    {
                        Id = "ProductImage",
                        Name = "ProductImage",
                        Description = "Image URL for this product (less than 200 characters).",
                        Type = PropertyType.String,
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
                        Description = "How much this product weighs.",
                        Type = PropertyType.Float,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = false
                    },
                    new Property
                    {
                        Id = "Cost",
                        Name = "Cost",
                        Description = "How much the product costs to make/build.",
                        Type = PropertyType.Float,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = false
                    },
                    new Property
                    {
                        Id = "Price",
                        Name = "Price",
                        Description = "How much you sell the product for.",
                        Type = PropertyType.Float,
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
        
        private class UpsertProductsCategoriesEndpoint : Endpoint
        {
            protected override string WritePathPropertyId { get; set; } = "CategoryID";
            protected override List<string> RequiredWritePropertyIds { get; set; } = new List<string>
            {
                "Name"
            };
            
            public override bool ShouldGetStaticSchema { get; set; } = true;

            public override Task<Schema> GetStaticSchemaAsync(IApiClient apiClient, Schema schema)
            {
                schema.Description = @"";
                
                var properties = new List<Property>
                {
                    new Property
                    {
                        Id = "CategoryID",
                        Name = "CategoryID",
                        Description = "The CategoryID, if included will attempt to update the target, if not included will attempt to create a new product category.",
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
                        Description = "Name of the Product Category (less than 100 characters).",
                        Type = PropertyType.String,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = false
                    },
                    new Property
                    {
                        Id = "Description",
                        Name = "Description",
                        Description = "Description of the category (less than 200 characters).",
                        Type = PropertyType.String,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = false
                    },
                    new Property
                    {
                        Id = "Image",
                        Name = "Image",
                        Description = "Image URL for this category (less than 200 characters).",
                        Type = PropertyType.String,
                        IsKey = false,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = "",
                        IsNullable = false
                    },
                    new Property
                    {
                        Id = "URL",
                        Name = "URL",
                        Description = "URL for this category (less than 200 characters).",
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
            {"UpsertProducts", new UpsertProductsEndpoint
            {
                Id = "UpsertProducts",
                Name = "Upsert Products",
                BasePath = "/Products",
                AllPath = "/",
                DetailPath = "/",
                DetailPropertyId = "ProductID",
                SupportedActions = new List<EndpointActions>
                {
                    EndpointActions.Post,
                    EndpointActions.Put
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
            {"UpsertProductCategories", new UpsertProductsCategoriesEndpoint
            {
                Id = "UpsertProductCategories",
                Name = "Upsert Product Categories",
                BasePath = "/ProductCategories",
                AllPath = "/",
                DetailPath = "/",
                DetailPropertyId = "CategoryID",
                SupportedActions = new List<EndpointActions>
                {
                    EndpointActions.Post,
                    EndpointActions.Put
                },
                PropertyKeys = new List<string>
                {
                    "CategoryID"
                }
            }},
        };
    }
}