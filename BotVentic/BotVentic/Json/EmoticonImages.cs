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

    class BttvEmoticon
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }
    }

    class BttvEmoticonImages
    {
        [JsonProperty("emotes")]
        public List<BttvEmoticon> Emotes { get; set; }

        [JsonProperty("urlTemplate")]
        public string Template { get; set; }
    }
    
    class FFZLinks
    {
        [JsonProperty("next")]
        public string Next { get; set; }
    }

    class FFZEmoticon
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Code { get; set; }
    }

    class FFZEmoticonImages
    {
        [JsonProperty("_links")]
        public FFZLinks Links { get; set; }

        [JsonProperty("emoticons")]
        public List<FFZEmoticon> Emotes { get; set; }
    }

    class FFZEmoticonSets
    {
        [JsonProperty("sets")]
        public Dictionary<string, FFZEmoticonImages> Sets { get; set; }
    }
}
