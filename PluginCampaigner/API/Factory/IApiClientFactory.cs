using PluginCampaigner.Helper;

namespace PluginCampaigner.API.Factory
{
    public interface IApiClientFactory
    {
        IApiClient CreateApiClient(Settings settings);
    }
}