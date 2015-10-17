using Newtonsoft.Json;
using System;

namespace BotVentic.Json
{
    class Channel
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [JsonProperty("partner")]
        public bool IsPartner { get; set; }

        [JsonProperty("followers")]
        public int Followers { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("created_at")]
        public DateTime Registered { get; set; }
    }

    class Stream
    {
        [JsonProperty("game")]
        public string Game { get; set; }

        [JsonProperty("viewers")]
        public int Viewers { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("video_height")]
        public int VideoHeight { get; set; }

        [JsonProperty("average_fps")]
        public double FramesPerSecond { get; set; }

        [JsonProperty("is_playlist")]
        public bool IsPlaylist { get; set; }

        [JsonProperty("channel")]
        public Channel Channel { get; set; }
    }

    class Streams
    {
        [JsonProperty("stream")]
        public Stream Stream { get; set; }
    }
}
