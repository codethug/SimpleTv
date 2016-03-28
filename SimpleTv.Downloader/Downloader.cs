using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using SimpleTv.Sdk;
using SimpleTv.Sdk.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace SimpleTv.Downloader
{
    public class Downloader
    {
        private ApplicationArguments arguments;
        private List<HttpData> httpLogs;
        SimpleTvClient client;

        public Downloader(ApplicationArguments arguments)
        {
            this.arguments = arguments;
            httpLogs = new List<HttpData>();
            client = new SimpleTvClient();
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
            var data = JsonConvert.SerializeObject(new { error = error, http = httpLogs });
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
                if (client.Login(arguments.Username, arguments.Password))
                {
                    foreach (var server in client.MediaServers)
                    {
                        var filteredShows = server.Shows.Where(s =>
                            Operators.LikeString(s.Name, arguments.ShowFilter, CompareMethod.Text)
                        );

                        Console.WriteLine();
                        foreach (var show in filteredShows)
                        {
                            Console.WriteLine(string.Format("Downloading {0} episodes of {1}", show.Episodes.Count, show.Name));
                            Console.WriteLine("=======================================================");

                            foreach (var episode in show.Episodes)
                            {
                                episode.Download(arguments.DownloadFolder, arguments.FolderFormat, arguments.FilenameFormat);
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
        }
    }
}
