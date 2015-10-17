using BotVentic.Json;
using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace BotVentic
{
    class Program
    {
        public static List<Emoticon> Emotes { get; private set; }
        public static List<BttvEmoticon> BttvEmotes { get; private set; }
        public static string BttvTemplate { get; private set; }

        static void Main(string[] args)
        {
            Config config;
            if (File.Exists("config.json"))
            {
                using (StreamReader sr = new StreamReader("config.json"))
                {
                    config = JsonConvert.DeserializeObject<Config>(sr.ReadToEnd());
                }
            }
            else
            {
                Console.WriteLine("No config file present!");
                System.Threading.Thread.Sleep(4000);
                return;
            }

            Console.WriteLine("Started!");
            UpdateEmotes();
            UpdateBttvEmotes();
            Console.WriteLine("Emotes acquired!");

            var client = new DiscordClient(new DiscordClientConfig());

            client.MessageCreated += MessageHandler.HandleIncomingMessage;

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
        /// Update the list of emoticons
        /// </summary>
        public static void UpdateEmotes()
        {
            var emotes = JsonConvert.DeserializeObject<EmoticonImages>(Request("https://api.twitch.tv/kraken/chat/emoticon_images"));
            Emotes = emotes.Emotes;
        }

        /// <summary>
        /// Update list of betterttv emoticons
        /// </summary>
        public static void UpdateBttvEmotes()
        {
            var emotes = JsonConvert.DeserializeObject<BttvEmoticonImages>(Request("https://api.betterttv.net/2/emotes"));
            BttvEmotes = emotes.Emotes;
            BttvTemplate = emotes.Template;
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
