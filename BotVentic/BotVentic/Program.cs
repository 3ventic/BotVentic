using BotVentic.Json;
using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace BotVentic
{
    class Program
    {
        private enum ConnectionState
        {
            Connecting,
            Connected,
            Disconnected
        }

        private static ConnectionState State = ConnectionState.Disconnected;

        // DictEmotes <EmoteCode, { emote_id, emote_type }>
        public static List<EmoteInfo> Emotes { get; private set; } = new List<EmoteInfo>();
        public static string BttvTemplate { get; private set; }

        public static int EditThreshold
        {
            get
            {
                return Config.EditThreshold;
            }
        }
        public static int EditMax
        {
            get
            {
                return Config.EditMax;
            }
        }

        private static DiscordClient Client { get; set; }
        private static Config Config { get; set; }
        public static string AuthUrl { get; private set; }

        private static object _lock = new object();
        private static bool UpdatingEmotes = false;

        static void Main(string[] args)
        {
            Console.WriteLine("Version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());

            if (File.Exists("config.json"))
            {
                using (StreamReader sr = new StreamReader("config.json"))
                {
                    Config = JsonConvert.DeserializeObject<Config>(sr.ReadToEnd());
                }
            }
            else
            {
                Console.WriteLine("No config file present! Please create a file called config.json in the program's working directory. See config.sample.json for a base.");
                Thread.Sleep(4000);
                return;
            }

            AuthUrl = Config.AuthUrl;

            Console.WriteLine("Started!");

            Task emoteUpdate = UpdateAllEmotesAsync();

            KeepConnectionAlive();
            emoteUpdate.Wait();
        }

        private static void KeepConnectionAlive()
        {
            while (true)
            {
                ConnectAsync();
                while (State != ConnectionState.Disconnected)
                    Thread.Sleep(1000);
                Thread.Sleep(5000);
            }
        }

        private static async void ConnectAsync()
        {
            State = ConnectionState.Connecting;

            Client = new DiscordClient();

            Client.MessageReceived += MessageHandler.HandleIncomingMessage;
            Client.MessageUpdated += MessageHandler.HandleEdit;

            Console.WriteLine("Connecting...");
            try
            {
                await Client.Connect(Config.Token);
                Client.SetGame("on github.com/3ventic/BotVentic");
                State = ConnectionState.Connected;
                Console.WriteLine("Connected!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Reconnecting...");
                State = ConnectionState.Disconnected;
                Client.Dispose();
            }
        }

        /// <summary>
        /// Update the list of all emoticons
        /// </summary>
        public static async Task UpdateAllEmotesAsync()
        {
            lock (_lock)
            {
                if (UpdatingEmotes)
                    return;
                else
                    UpdatingEmotes = true;
            }
            Console.WriteLine("Loading emotes!");

            if (Emotes == null)
                Emotes = new List<EmoteInfo>();

            List<EmoteInfo> emotes = new List<EmoteInfo>();
            await UpdateFFZEmotes(emotes);
            await UpdateBttvEmotes(emotes);
            await UpdateTwitchEmotes(emotes);
            Emotes = emotes;
            UpdatingEmotes = false;

            Console.WriteLine("Emotes acquired!");
        }

        /// <summary>
        /// Update the list of emoticons
        /// </summary>
        private static async Task UpdateTwitchEmotes(List<EmoteInfo> e)
        {
            var emotes = JsonConvert.DeserializeObject<EmoticonImages>(await RequestAsync("https://api.twitch.tv/kraken/chat/emoticon_images"));

            if (emotes == null || emotes.Emotes == null)
            {
                Console.WriteLine("Error loading twitch emotes!");
                return;
            }

            emotes.Emotes.Sort((a, b) =>
            {
                int aSet = 0;
                int bSet = 0;

                if (a != null && a.Set != null)
                    aSet = a.Set ?? 0;
                if (b != null && b.Set != null)
                    bSet = b.Set ?? 0;

                if (aSet == bSet)
                    return 0;

                if (aSet == 0)
                    return 1;

                if (bSet == 0)
                    return -1;

                return aSet - bSet;
            });

            foreach (var em in emotes.Emotes)
            {
                e.Add(new EmoteInfo(em.Id, em.Code, EmoteType.Twitch, em.Set ?? 0));
            }
        }

        /// <summary>
        /// Update list of betterttv emoticons
        /// </summary>
        private static async Task UpdateBttvEmotes(List<EmoteInfo> e)
        {
            var emotes = JsonConvert.DeserializeObject<BttvEmoticonImages>(await RequestAsync("https://api.betterttv.net/2/emotes"));

            if (emotes == null || emotes.Template == null || emotes.Emotes == null)
            {
                Console.WriteLine("Error loading bttv emotes");
                return;
            }

            BttvTemplate = emotes.Template;

            foreach (var em in emotes.Emotes)
            {
                e.Add(new EmoteInfo(em.Id, em.Code, EmoteType.Bttv));
            }
        }


        /// <summary>
        /// Update the list of FrankerFaceZ emoticons
        /// </summary>
        private static async Task UpdateFFZEmotes(List<EmoteInfo> e)
        {
            var emotes = JsonConvert.DeserializeObject<FFZEmoticonSets>(await RequestAsync("http://api.frankerfacez.com/v1/set/global"));

            if (emotes == null || emotes.Sets == null || emotes.Sets.Values == null)
            {
                Console.WriteLine("Error loading ffz emotes");
                return;
            }

            foreach (FFZEmoticonImages set in emotes.Sets.Values)
            {
                if (set != null && set.Emotes != null)
                {
                    foreach (var em in set.Emotes)
                    {
                        e.Add(new EmoteInfo(em.Id, em.Code, EmoteType.Ffz));
                    }
                }
            }
        }


        /// <summary>
        /// Get URL
        /// </summary>
        /// <param name="uri">URL to request</param>
        /// <returns>Response body</returns>
        public static async Task<string> RequestAsync(string uri)
        {
            WebRequest request = WebRequest.Create(uri);
            // 30 seconds max, mainly because of emotes
            request.Timeout = 15000;

            // Change our user agent string to something more informative
            ((HttpWebRequest) request).UserAgent = "BotVentic/1.0";
            try
            {
                string data;
                using (WebResponse response = await request.GetResponseAsync())
                {
                    using (System.IO.Stream stream = response.GetResponseStream())
                    {
                        System.IO.StreamReader reader = new System.IO.StreamReader(stream);
                        data = reader.ReadToEnd();
                    }
                }
                return data;
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
