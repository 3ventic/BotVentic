using Newtonsoft.Json;

namespace BotVentic.Json
{
    class Config
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
