using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PluginCampaigner.API.Utility;
using PluginCampaigner.DataContracts;
using PluginCampaigner.Helper;

namespace PluginCampaigner.API.Factory
{
    public class ApiClient: IApiClient
    {
        private IApiAuthenticator Authenticator { get; set; }
        private HttpClient Client { get; set; }
        private Settings Settings { get; set; }

        public ApiClient(HttpClient client, Settings settings)
        {
            Authenticator = new ApiAuthenticator(client, settings);
            Client = client;
            Settings = settings;
        }
        
        public async Task TestConnection()
        {
            try
            {
                var token = await Authenticator.GetToken();
                var uri = $"{Constants.BaseApiUrl.TrimEnd('/')}/{Utility.Constants.TestConnectionPath}";
                
                Client.DefaultRequestHeaders.Clear();
                Client.DefaultRequestHeaders.Add("ApiKey", token);
                Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await Client.GetAsync(uri);
                if (!response.IsSuccessStatusCode)
                {
                    var apiError =
                        JsonConvert.DeserializeObject<ApiError>(await response.Content.ReadAsStringAsync());
                    throw new Exception(apiError.Error);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> GetAsync(string path)
        {
            try
            {
                var token = await Authenticator.GetToken();
                var uri = $"{Constants.BaseApiUrl.TrimEnd('/')}/{path.TrimStart('/')}";
                
                Client.DefaultRequestHeaders.Clear();
                Client.DefaultRequestHeaders.Add("ApiKey", token);
                Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await Client.GetAsync(uri);
                // if (!response.IsSuccessStatusCode)
                // {
                //     var apiError =
                //         JsonConvert.DeserializeObject<ApiError>(await response.Content.ReadAsStringAsync());
                //     throw new Exception(apiError.Error);
                // }

                return response;
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> PostAsync(string path, StringContent json)
        {
            try
            {
                var token = await Authenticator.GetToken();
                var uri = new Uri($"{Constants.BaseApiUrl.TrimEnd('/')}/{path.TrimStart('/')}");
                
                Client.DefaultRequestHeaders.Clear();
                Client.DefaultRequestHeaders.Add("ApiKey", token);
                Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await Client.PostAsync(uri, json);
                // if (!response.IsSuccessStatusCode)
                // {
                //     var apiError =
                //         JsonConvert.DeserializeObject<ApiError>(await response.Content.ReadAsStringAsync());
                //     throw new Exception(apiError.Error);
                // }

                return response;
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> PutAsync(string path, StringContent json)
        {
            try
            {
                var token = await Authenticator.GetToken();
                var uri = new Uri($"{Constants.BaseApiUrl.TrimEnd('/')}/{path.TrimStart('/')}");
                
                Client.DefaultRequestHeaders.Clear();
                Client.DefaultRequestHeaders.Add("ApiKey", token);
                Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await Client.PutAsync(uri, json);
                // if (!response.IsSuccessStatusCode)
                // {
                //     var apiError =
                //         JsonConvert.DeserializeObject<ApiError>(await response.Content.ReadAsStringAsync());
                //     throw new Exception(apiError.Error);
                // }

                return response;
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> PatchAsync(string path, StringContent json)
        {
            try
            {
                var token = await Authenticator.GetToken();
                var uri = new Uri($"{Constants.BaseApiUrl.TrimEnd('/')}/{path.TrimStart('/')}");
                
                Client.DefaultRequestHeaders.Clear();
                Client.DefaultRequestHeaders.Add("ApiKey", token);
                Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await Client.PatchAsync(uri, json);
                // if (!response.IsSuccessStatusCode)
                // {
                //     var apiError =
                //         JsonConvert.DeserializeObject<ApiError>(await response.Content.ReadAsStringAsync());
                //     throw new Exception(apiError.Error);
                // }

                return response;
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> DeleteAsync(string path)
        {
            try
            {
                var token = await Authenticator.GetToken();
                var uri = $"{Constants.BaseApiUrl.TrimEnd('/')}/{path.TrimStart('/')}";
                
                Client.DefaultRequestHeaders.Clear();
                Client.DefaultRequestHeaders.Add("ApiKey", token);
                Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await Client.DeleteAsync(uri);
                // if (!response.IsSuccessStatusCode)
                // {
                //     var apiError =
                //         JsonConvert.DeserializeObject<ApiError>(await response.Content.ReadAsStringAsync());
                //     throw new Exception(apiError.Error);
                // }

                return response;
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
        }
    }
}