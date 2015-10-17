using Newtonsoft.Json;

namespace BotVentic.Json
{
    class Config
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
