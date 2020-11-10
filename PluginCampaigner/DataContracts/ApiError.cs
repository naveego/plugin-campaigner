using Newtonsoft.Json;

namespace PluginCampaigner.DataContracts
{
    public class ApiError
    {
        [JsonProperty("error")]
        public string Error { get; set; }
    }
}