using System.Threading.Tasks;

namespace PluginCampaigner.API.Factory
{
    public interface IApiAuthenticator
    {
        Task<string> GetToken();
    }
}