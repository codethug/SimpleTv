using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using SimpleTv.Sdk;
using SimpleTv.Sdk.Diagnostics;
using SimpleTv.Sdk.Naming;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace SimpleTv.Downloader
{
    public class Downloader
    {
        private Configuration config;
        private List<HttpData> httpLogs;
        SimpleTvClient client;

        public Downloader(Configuration config)
        {
            this.config = config;
            httpLogs = new List<HttpData>();
            client = new SimpleTvClient(config);
            client.HttpResponseReceived += Client_HttpResponseReceived;
        }

        private void Client_HttpResponseReceived(object sender, HttpResponseReceivedEventArgs e)
        {
            if (e.HttpData != null)
            {
                httpLogs.Add(e.HttpData);
            }
        }

        public void SaveHttpLogs(string error)
        {
            var data = JsonConvert.SerializeObject(new {
                what = new {
                    name = "SimpleTV Downloader Logs",
                    version = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                },
                error = error,
                http = httpLogs
            });
            var fileName = string.Format("SimpleTv.DownloadLog.{0:yyyy-MM-dd_HH-mm-ss}.json.txt", DateTime.UtcNow);

            // First try the current directory
            string fullFileName = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            try
            {
                File.WriteAllText(fullFileName, data);
            }
            catch (UnauthorizedAccessException e)
            {
                // IF the current directory fails, then try the TEMP directory
                fullFileName = Path.Combine(Path.GetTempPath(), fileName);
                File.WriteAllText(fullFileName, data);
            }

            Console.WriteLine("The HTTP logs have been saved to " + fullFileName);
            Console.WriteLine();
        }

        public void Download()
        {
            Console.WriteLine();
            try
            {
                if (client.Login(config.Username, config.Password))
                {
                    foreach (var server in client.MediaServers)
                    {
                        var filteredShows = client
                            .GetShows(server)
                            .IncludeOnly(config.IncludeFilter)
                            .Exclude(config.ExcludeFilter);

                        Console.WriteLine();
                        foreach (var show in filteredShows)
                        {
                            var episodes = client.GetEpisodes(show);

                            Console.WriteLine(string.Format("Downloading {0} episode{1} of \"{2}\"", 
                                episodes.Count, episodes.Count == 1 ? string.Empty : "s", show.Name));
                            Console.WriteLine("=======================================================");

                            foreach (var episode in episodes)
                            {
                                client.DownloadEpisode(episode);
                            }

                            Console.WriteLine("=======================================================");
                            Console.WriteLine("Finished downloading " + show.Name);
                            Console.WriteLine();
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Login Failed");
                }

            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.NameResolutionFailure)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Are you sure you're connected to the internet?  I'm having trouble with DNS resolution.");
                    Console.ResetColor();
                }
            }
            catch (DenyStreamingException e)
            {
                Console.WriteLine();
                StvConsole.WriteError(e.Message);
            }
        }
    }
}
