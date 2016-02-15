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
        public readonly string Code;
        public readonly EmoteType Type;
        public readonly int EmoteSet;

        public EmoteInfo(int id, string code, EmoteType type, int set = -1)
        {
            Id = id.ToString();
            Code = code;
            Type = type;
            EmoteSet = set;
        }

        public EmoteInfo(string id, string code, EmoteType type, int set = -1)
        {
            Id = id;
            Code = code;
            Type = type;
            EmoteSet = set;
        }
    }
}
