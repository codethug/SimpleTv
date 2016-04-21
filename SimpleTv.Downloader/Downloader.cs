using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using NodaTime;
using SimpleTv.Sdk;
using SimpleTv.Sdk.Diagnostics;
using SimpleTv.Sdk.Http;
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
        SimpleTvClient tvClient;

        public Downloader(Configuration config)
        {
            this.config = config;
            httpLogs = new List<HttpData>();

            var webClient = new CookieAwareWebClient();
            var docClient = new HtmlDocumentClient(webClient);
            var tvHttpClient = new SimpleTvHttpClient(SystemClock.Instance, DateTimeZoneProviders.Bcl, webClient, docClient);
            tvHttpClient.HttpResponseReceived += Client_HttpResponseReceived;

            tvClient = new SimpleTvClient(tvHttpClient);
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
            catch (UnauthorizedAccessException)
            {
                // Since the current directory failed, use the TEMP directory
                fullFileName = Path.Combine(Path.GetTempPath(), fileName);
                File.WriteAllText(fullFileName, data);
            }

            Console.WriteLine("The HTTP logs have been saved to " + fullFileName);
            Console.WriteLine();
        }

        public void Reboot()
        {
            Console.WriteLine();
            try
            {
                if (tvClient.Login(config.Username, config.Password))
                {
                    foreach (var server in tvClient.GetMediaServers(config.ServerIncludeFilter, config.ServerExcludeFilter))
                    {
                        Console.WriteLine();
                        Console.Write("Are you sure you want to reboot \"" + server.Name + "\" [y/N] ");
                        if (Console.ReadLine().ToUpper() == "Y")
                        {
                            tvClient.Reboot(server);
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
                throw;
            }
        }

        public void Download()
        {
            Console.WriteLine();
            try
            {
                if (tvClient.Login(config.Username, config.Password))
                {
                    foreach (var server in tvClient.GetMediaServers(config.ServerIncludeFilter, config.ServerExcludeFilter))
                    {
                        Console.WriteLine();
                        foreach (var show in tvClient.GetShows(server, config.ShowIncludeFilter, config.ShowExcludeFilter))
                        {
                            var episodes = tvClient.GetEpisodes(show);

                            Console.WriteLine(string.Format("Downloading {0} episode{1} of \"{2}\"", 
                                episodes.Count, episodes.Count == 1 ? string.Empty : "s", show.Name));
                            Console.WriteLine("=======================================================");

                            foreach (var episode in episodes)
                            {
                                try
                                {
                                    tvClient.DownloadEpisode(episode, config.DownloadFolder, config.FolderFormat, config.FilenameFormat);
                                }
                                catch (StreamNotFoundException e)
                                {
                                    StvConsole.WriteError(e.Message);
                                }
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
                throw;
            }
            catch (DenyStreamingException e)
            {
                Console.WriteLine();
                StvConsole.WriteError(e.Message);
            }
        }
    }
}
