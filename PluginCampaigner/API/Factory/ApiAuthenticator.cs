using System.Net.Http;
using System.Threading.Tasks;
using PluginCampaigner.Helper;

namespace PluginCampaigner.API.Factory
{
    public class ApiAuthenticator: IApiAuthenticator
    {
        private HttpClient Client { get; set; }
        private Settings Settings { get; set; }
        
        public ApiAuthenticator(HttpClient client, Settings settings)
        {
            Client = client;
            Settings = settings;
        }

        public Task<string> GetToken()
        {
            return GetNewToken();
        }

        private Task<string> GetNewToken()
        {
            return Task.FromResult(Settings.ApiKey);
        }
    }
}