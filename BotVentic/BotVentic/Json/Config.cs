using Newtonsoft.Json;

namespace BotVentic.Json
{
    class Config
    {
        [JsonProperty("auth_url")]
        public string AuthUrl { get; set; } = "https://discordapp.com/oauth2/authorize?client_id=174449568304332800&scope=bot&permissions=19456";

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("editthreshold")]
        public int EditThreshold { get; set; } = 1;

        [JsonProperty("editmax")]
        public int EditMax { get; set; } = 10;
    }
}
