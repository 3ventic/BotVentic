namespace BotVentic
{
    enum EmoteType
    {
        Twitch,
        Bttv,
        Ffz
    }

    struct EmoteInfo
    {
        public readonly string Id;
        public readonly EmoteType Type;

        public EmoteInfo(int id, EmoteType type)
        {
            Id = id.ToString();
            Type = type;
        }

        public EmoteInfo(string id, EmoteType type)
        {
            Id = id;
            Type = type;
        }
    }
}
