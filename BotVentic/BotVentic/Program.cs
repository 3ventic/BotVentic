using BotVentic.Json;
using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace BotVentic
{
    class Program
    {
        // DictEmotes <EmoteCode, { emote_id, emote_type }>
        public static Dictionary<string, string[]> DictEmotes { get; private set; }
        public static string BttvTemplate { get; private set; }

        public static int EditThreshold { get; set; }
        public static int EditMax { get; set; }

        static void Main(string[] args)
        {
            Console.WriteLine("Version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            DictEmotes = new Dictionary<string, string[]>();

            Config config;
            if (File.Exists("config.json"))
            {
                using (StreamReader sr = new StreamReader("config.json"))
                {
                    config = JsonConvert.DeserializeObject<Config>(sr.ReadToEnd());
                    EditThreshold = config.EditThreshold;
                    EditMax = config.EditMax;
                }
            }
            else
            {
                Console.WriteLine("No config file present!");
                System.Threading.Thread.Sleep(4000);
                return;
            }

            Console.WriteLine("Started!");

            UpdateAllEmotes();

            Console.WriteLine("Emotes acquired!");

            var client = new DiscordClient(new DiscordClientConfig());

            client.MessageCreated += MessageHandler.HandleIncomingMessage;
            client.MessageUpdated += MessageHandler.HandleEdit;

            client.Run(async () =>
            {
                Console.WriteLine("Connecting...");
                try
                {
                    await client.Connect(config.Email, config.Password);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return;
                }
                Console.WriteLine("Connected!");
            });
            Console.WriteLine("Press Any key to quit");
            Console.ReadKey();
        }

        /// <summary>
        /// Update the list of all emoticons
        /// </summary>
        public static void UpdateAllEmotes()
        {
            DictEmotes.Clear();
            UpdateEmotes();
            UpdateBttvEmotes();
            UpdateFFZEmotes();
        }

        /// <summary>
        /// Update the list of emoticons
        /// </summary>
        public static void UpdateEmotes()
        {
            var emotes = JsonConvert.DeserializeObject<EmoticonImages>(Request("https://api.twitch.tv/kraken/chat/emoticon_images"));
            //Emotes = emotes.Emotes;
            foreach (var em in emotes.Emotes)
            {
                try
                {
                    DictEmotes.Add(em.Code, new string[] { "" + em.Id, "twitch" });
                }
                catch { }
            }
        }

        /// <summary>
        /// Update list of betterttv emoticons
        /// </summary>
        public static void UpdateBttvEmotes()
        {
            var emotes = JsonConvert.DeserializeObject<BttvEmoticonImages>(Request("https://api.betterttv.net/2/emotes"));
            //BttvEmotes = emotes.Emotes;
            foreach (var em in emotes.Emotes)
            {
                try
                {
                    DictEmotes.Add(em.Code, new string[] { "" + em.Id, "bttv" });
                }
                catch { }
            }
        }


        /// <summary>
        /// Update the list of FrankerFaceZ emoticons
        /// </summary>
        public static void UpdateFFZEmotes()
        {
            List<FFZEmoticon> FFZEmotes = new List<FFZEmoticon>();
            var emotes = JsonConvert.DeserializeObject<FFZEmoticonSets>(Request("http://api.frankerfacez.com/v1/set/global"));
            foreach (FFZEmoticonImages set in emotes.Sets.Values)
            {
                foreach (var em in set.Emotes)
                {
                    try
                    {
                        DictEmotes.Add(em.Code, new string[] { "" + em.Id, "ffz" });
                    }
                    catch { }
                }
            }
        }


        /// <summary>
        /// Get URL
        /// </summary>
        /// <param name="uri">URL to request</param>
        /// <returns>Response body</returns>
        public static string Request(string uri)
        {
            WebRequest request = WebRequest.Create(uri);
            // 30 seconds max, mainly because of emotes
            request.Timeout = 15000;

            // Change our user agent string to something more informative
            ((HttpWebRequest)request).UserAgent = "BotVentic/1.0";
            try
            {
                string data;
                using (WebResponse response = request.GetResponse())
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
