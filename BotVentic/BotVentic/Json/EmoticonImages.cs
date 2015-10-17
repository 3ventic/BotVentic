using System.Collections.Generic;
using Newtonsoft.Json;

namespace BotVentic.Json
{
    class Emoticon
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }
    }

    class EmoticonImages
    {
        [JsonProperty("emoticons")]
        public List<Emoticon> Emotes { get; set; }
    }
}
