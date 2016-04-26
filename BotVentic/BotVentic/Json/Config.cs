using Newtonsoft.Json;

namespace BotVentic.Json
{
    class Config
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("editthreshold")]
        public int EditThreshold { get; set; } = 1;

        [JsonProperty("editmax")]
        public int EditMax { get; set; } = 10;
    }
}
