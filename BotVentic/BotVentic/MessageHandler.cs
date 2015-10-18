using Discord;
using Newtonsoft.Json;
using System;

namespace BotVentic
{
    class MessageHandler
    {
        public static async void HandleIncomingMessage(object client, MessageEventArgs e)
        {
            if (e != null && e.Message != null && !e.Message.IsAuthor)
            {
                string server = e.Message.Server == null ? "1-1" : e.Message.Server.Name;
                string user = e.Message.User == null ? "?" : e.Message.User.Name;
                Console.WriteLine("[" + server + "][Message] " + user + ": " + e.Message.RawText);
                string reply = null;
                string[] words = e.Message.RawText.Split(' ');

                if (words[0] == "invite" && words.Length >= 2)
                    await ((DiscordClient)client).AcceptInvite(words[1]);

                reply = HandleCommands(reply, words);

                if (reply == null)
                    reply = HandleEmotesAndConversions(reply, words);

                if (!String.IsNullOrWhiteSpace(reply))
                    await ((DiscordClient)client).SendMessage(e.Message.ChannelId, reply);
            }
        }

        public static async void HandleEdits(object client, MessageEventArgs e)
        {
            if (e != null && e.Message != null)
            {
                Console.WriteLine("Words being Edited");
                bool calcDate = (DateTime.Now - e.Message.Timestamp).Minutes < Program.EditThreshold;
                string server = e.Message.Server == null ? "101" : e.Message.Server.Name;
                string user = e.Message.User == null ? "?" : e.Message.User.Name;
                Console.WriteLine(String.Format("[{0}][Edit] {1}: {2}", server, user, e.Message.RawText));
                string reply = null;
                string[] words = e.Message.RawText.Split(' ');

                reply = HandleCommands(reply, words);

                if (reply == null)
                {
                    reply = HandleEmotesAndConversions(reply, words);
                }

                if (!String.IsNullOrWhiteSpace(reply))
                {
                    await ((DiscordClient)client).SendMessage(e.Message.ChannelId, reply);
                }
            }
        }

        private static string HandleEmotesAndConversions(string reply, string[] words)
        {
            for (int i = words.Length - 1; i >= 0; --i)
            {
                string word = words[i];
                bool found = false;
                if (word.StartsWith("#"))
                {
                    string code = word.Substring(1, word.Length - 1);
                    found = IsWordEmote(code, ref reply);
                }
                else if (word.StartsWith(":") && word.EndsWith(":"))
                {
                    string code = word.Substring(1, word.Length - 2);
                    found = IsWordEmote(code, ref reply, false);
                }
                if (found)
                    break;

                switch (word)
                {
                    case "C":
                        if (i >= 1)
                        {
                            int celsius;
                            if (Int32.TryParse(words[i - 1], out celsius))
                            {
                                reply = celsius + " \u00b0C = " + (celsius * 9 / 5 + 32) + " \u00b0F";
                            }
                        }
                        break;
                    case "F":
                        if (i >= 1)
                        {
                            int fahrenheit;
                            if (Int32.TryParse(words[i - 1], out fahrenheit))
                            {
                                reply = fahrenheit + " \u00b0F = " + ((fahrenheit - 32) * 5 / 9) + " \u00b0C";
                            }
                        }
                        break;
                }
            }

            return reply;
        }


        private static bool IsWordEmote(string code, ref string reply, bool caseSensitive = true)
        {
            Func<string, string, bool> emoteComparer = (first, second) => { return caseSensitive ? (first == second) : (first.ToLower() == second.ToLower()); };
            bool found = false;

            foreach (var emote in Program.Emotes)
            {
                if (emoteComparer(code, emote.Code))
                {
                    reply = "http://emote.3v.fi/2.0/" + emote.Id + ".png";
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                foreach (var emote in Program.BttvEmotes)
                {
                    if (emoteComparer(code, emote.Code))
                    {
                        reply = "https:" + Program.BttvTemplate.Replace("{{id}}", emote.Id).Replace("{{image}}", "2x");
                        found = true;
                        break;
                    }
                }
            }

            if (!found)
            {
                foreach (var emote in Program.FFZEmotes)
                {
                    if (emoteComparer(code, emote.Code))
                    {
                        Console.WriteLine(emote.Code);
                        reply = "http://cdn.frankerfacez.com/emoticon/" + emote.Id + "/2";
                        found = true;
                        break;
                    }
                }
            }

            return found;
        }


        private static string HandleCommands(string reply, string[] words)
        {
            switch (words[0])
            {
                case "!stream":
                    if (words.Length > 1)
                    {
                        var streams = JsonConvert.DeserializeObject<Json.Streams>(Program.Request("https://api.twitch.tv/kraken/streams/" + words[1].ToLower() + "?stream_type=all"));
                        if (streams.Stream == null)
                        {
                            reply = "The channel is currently *offline*";
                        }
                        else
                        {
                            long ticks = DateTime.UtcNow.Ticks - streams.Stream.CreatedAt.Ticks;
                            TimeSpan ts = new TimeSpan(ticks);
                            reply = "**[" + streams.Stream.Channel.DisplayName + "]**" + (streams.Stream.Channel.IsPartner ? @"\*" : "") + " " + (streams.Stream.IsPlaylist ? "(Playlist)" : "")
                                + "\n**Title**: " + streams.Stream.Channel.Status.Replace("*", @"\*")
                                + "\n**Game:** " + streams.Stream.Game + "\n**Viewers**: " + streams.Stream.Viewers
                                + "\n**Uptime**: " + ts.ToString(@"d' day" + (ts.Days == 1 ? "" : "s") + @" 'hh\:mm\:ss")
                                + "\n**Quality**: " + streams.Stream.VideoHeight + "p" + Math.Ceiling(streams.Stream.FramesPerSecond);
                        }
                    }
                    else
                    {
                        reply = "**Usage:** !stream channel";
                    }
                    break;
                case "!channel":
                    if (words.Length > 1)
                    {
                        var channel = JsonConvert.DeserializeObject<Json.Channel>(Program.Request("https://api.twitch.tv/kraken/channels/" + words[1].ToLower()));
                        reply = "**[" + channel.DisplayName + "]**"
                            + "\n**Partner**: " + (channel.IsPartner ? "Yes" : "No")
                            + "\n**Title**: " + channel.Status.Replace("*", @"\*")
                            + "\n**Registered**: " + channel.Registered.ToString("yyyy-MM-dd HH:mm") + " UTC"
                            + "\n**Followers**: " + channel.Followers;
                    }
                    else
                    {
                        reply = "**Usage:** !channel channel";
                    }
                    break;
                case "!source":
                    reply = "https://github.com/3ventic/BotVentic";
                    break;
            }

            return reply;
        }
    }
}
