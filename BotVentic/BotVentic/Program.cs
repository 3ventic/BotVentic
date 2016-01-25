using BotVentic.Json;
using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace BotVentic
{
    class Program
    {
        // DictEmotes <EmoteCode, { emote_id, emote_type }>
        public static ConcurrentDictionary<string, EmoteInfo> DictEmotes { get; private set; } = new ConcurrentDictionary<string, EmoteInfo>();
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
                Console.WriteLine("No config file present!");
                System.Threading.Thread.Sleep(4000);
                return;
            }

            Console.WriteLine("Started!");

            Task emoteUpdate = UpdateAllEmotesAsync();

            Console.WriteLine("Emotes acquired!");

            Client = new DiscordClient(new DiscordClientConfig());

            Client.MessageCreated += MessageHandler.HandleIncomingMessage;
            Client.MessageUpdated += MessageHandler.HandleEdit;
            Client.Disconnected += HandleDisconnect;

            ConnectLoop();
            emoteUpdate.Wait();
        }

        private static void HandleDisconnect(object sender, DisconnectedEventArgs e)
        {
            Console.WriteLine("Disconnected... Attempting to reconnect in 2 seconds.");
            Thread.Sleep(2000);
            ConnectLoop();
        }

        private static void ConnectLoop()
        {
            while (!Connect())
            {
                Thread.Sleep(1000);
            }
        }

        private static bool Connect()
        {
            Console.WriteLine("Connecting...");
            try
            {
                Client.Run(async () => { await Client.Connect(Config.Email, Config.Password); });
            }
            catch (Discord.TimeoutException)
            {
                Console.WriteLine("Connection attempt timed out. Reconnecting...");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Reconnecting...");
                return false;
            }
            Console.WriteLine("Connected!");
            return true;
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
            DictEmotes.Clear();
            await UpdateFFZEmotes();
            await UpdateBttvEmotes();
            await UpdateTwitchEmotes();
            UpdatingEmotes = false;
        }

        /// <summary>
        /// Update the list of emoticons
        /// </summary>
        private static async Task UpdateTwitchEmotes()
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
                DictEmotes[em.Code] = new EmoteInfo(em.Id, EmoteType.Twitch);
            }
        }

        /// <summary>
        /// Update list of betterttv emoticons
        /// </summary>
        private static async Task UpdateBttvEmotes()
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
                DictEmotes[em.Code] = new EmoteInfo(em.Id, EmoteType.Bttv);
            }
        }


        /// <summary>
        /// Update the list of FrankerFaceZ emoticons
        /// </summary>
        private static async Task UpdateFFZEmotes()
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
                        DictEmotes[em.Code] = new EmoteInfo(em.Id, EmoteType.Ffz);
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
