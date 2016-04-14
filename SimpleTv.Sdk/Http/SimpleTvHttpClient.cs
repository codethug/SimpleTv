using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

using HtmlAgilityPack;
using NodaTime;

using SimpleTv.Sdk.Models;
using SimpleTv.Sdk.Diagnostics;
using SimpleTv.Sdk.Naming;
using System.Collections.Specialized;

namespace SimpleTv.Sdk.Http
{
    public class SimpleTvHttpClient
    {
        private const string simpleTvBaseUrl = "https://us.simple.tv";
        private readonly IWebClient webClient;
        private readonly IHtmlDocumentClient docClient;
        private IClock clock;
        Object sync;
        private IDateTimeZoneProvider dtzProvider;

        internal SimpleTvHttpClient(IClock clock, IDateTimeZoneProvider dtzProvider, IWebClient webClient, IHtmlDocumentClient docClient)
        {
            sync = new Object();

            this.webClient = webClient;
            webClient.DownloadProgressChanged += Client_DownloadProgressChanged;
            webClient.DownloadFileCompleted += Client_DownloadFileCompleted;
            this.docClient = docClient;

            this.clock = clock;
            this.dtzProvider = dtzProvider;
        }

        public event EventHandler<HttpResponseReceivedEventArgs> HttpResponseReceived
        {
            add { docClient.HttpResponseReceived += value; }
            remove { docClient.HttpResponseReceived -= value; }
        }

        private int BrowserUTCOffsetMinutes
        {
            get
            {
                return dtzProvider
                    .GetSystemDefault()
                    .GetUtcOffset(clock.Now)
                    .Milliseconds / 1000 / 60;
            }
        }

        private string BrowserDateTimeUTC
        {
            get
            {
                return clock
                    .Now
                    .ToDateTimeUtc()
                    .ToString("yyyy/M/d HH:mm:ss");
            }
        }

        // POST to https://us.simple.tv/Auth/SignIn
        //   ?InvitationKey=&UserName=user%40gmail.com&Password=[EncodedPW]&RememberMe=false&browserDateTime=2016/3/11 19:28:16
        // Keep cookies for future requests
        internal bool Login(string un, string pw)
        {
            var uri = new Uri(simpleTvBaseUrl + "/Auth/SignIn");
            var rawResponse = docClient.PostRawReponse(uri, "Logging In", new NameValueCollection {
                { "InvitationKey", string.Empty },
                { "UserName", un },
                { "Password", pw },
                { "RememberMe", false.ToString() },
                { "browserDateTime", BrowserDateTimeUTC }
            });

            // Bad Login looks like this:
            // HTTP 200     {"SignInError":"Email Address or Password is incorrect."}
            dynamic response = Newtonsoft.Json.JsonConvert.DeserializeObject(rawResponse);
            if (response.SignInError != null)
            {
                return false;
            }
            return true;
        }

        internal List<MediaServer> GetMediaServers()
        {
            return docClient.GetDocument(new Uri("http://us-my.simple.tv"), "Loading Media Servers")
                .ParseMediaServers(this)
                .Where(ms =>
                {
                    // Find out where the DVR is on the internet/network
                    LocateMediaServer(ms);
                    // Ensure we can actually communicate with the DVR
                    return TestMediaServerLocations(ms);
                })
                .ToList();
        }

        private MediaServer LocateMediaServer(MediaServer server)
        {
            var description = string.Format("Locating Media Server \"{0}\"", server.Name);
            var urlTemplate = "https://us-my.simple.tv/Data/RealTimeData?accountId={0}&mediaServerId={1}&playerAlternativeAvailable=false";

            var url = new Uri(string.Format(urlTemplate, server.AccountId, server.Id));
            var rawResponse = docClient.GetRaw(url, description);
            dynamic response = Newtonsoft.Json.JsonConvert.DeserializeObject(rawResponse);

            // {
            //     "MediaServerId": "49535352-5453-3156-0005-100000001576",
            //     "InstanceState": [{
            //         "Action": 2,
            //         "Id": "3cc96b3b-ea80-4533-ae34-e416c4b70387",
            //         "InstanceId": "285240fd-e164-11e5-b973-22000b6981a6",
            //         "IsSeriesRecording": true,
            //         "State": 4
            //     }, ... ],
            //     "MediaServerStatus": 1,
            //     "StorageState": [{
            //         "Action": 2,
            //         "Capacity": 1999079391232,
            //         "FreeSpace": 1639157075968,
            //         "Id": "7fa7fa16-9e45-47a5-a7cf-ae2c863e0e11",
            //         "State": 2
            //     }],
            //     "StreamServer": {
            //         "MediaServerId": null,
            //         "LocalPingURL": "http://192.168.2.7/static/ping.gif",
            //         "LocalStreamBaseURL": "http://192.168.2.7/translate/rss/",
            //         "RemotePingURL": "http://54.174.83.84/relay/pingimage/mediaserver/49535352-5453-3156-0005-100000001576",
            //         "RemoteStreamBaseURL": "http://54.174.83.84/relay/mediaserver/49535352-5453-3156-0005-100000001576/rss"
            //     },
            //     "LocalPingURL": "http://192.168.2.7/static/ping.gif",
            //     "LocalStreamBaseURL": "http://192.168.2.7/translate/rss/",
            //     "RemotePingURL": "http://54.174.83.84/relay/pingimage/mediaserver/49535352-5453-3156-0005-100000001576",
            //     "RemoteStreamBaseURL": "http://54.174.83.84/relay/mediaserver/49535352-5453-3156-0005-100000001576/rss",
            //     "User": null
            // }

            server.LocalPingUrl = response.LocalPingURL;
            server.LocalStreamBaseUrl = response.LocalStreamBaseURL;
            server.RemotePingUrl = response.RemotePingURL;
            server.RemoteStreamBaseUrl = response.RemoteStreamBaseURL;

            return server;
        }

        private bool TestMediaServerLocations(MediaServer ms)
        {
            var description = string.Format("Testing Media Server \"{0}\"", ms.Name);
            Console.WriteLine(description);
            if (PingUrl(ms.LocalPingUrl))
            {
                ms.UseLocalStream = true;
                return true;
            }
            else if (PingUrl(ms.RemotePingUrl))
            {
                ms.UseLocalStream = false;
                return true;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("I can't communicate with your \"{0}\" DVR.  Please verify that you can play a show from your DVR at https://my.simple.tv.", ms.Name);
                Console.ResetColor();
                return false;
            }
        }

        private bool PingUrl(string url)
        {
            try
            {
                docClient.GetRaw(new Uri(url));
            }
            catch (WebException we)
            {
                if (we.Status == WebExceptionStatus.Timeout)
                {
                    return false;
                }
                throw;
            }
            return true;
        }

        internal List<Show> GetShows(MediaServer server)
        {
            var urlTemplate = "https://us-my.simple.tv/Library/MyShows?browserDateTimeUTC={0}&mediaServerID={1}&browserUTCOffsetMinutes={2}";
            var url = string.Format(urlTemplate, BrowserDateTimeUTC, server.Id, BrowserUTCOffsetMinutes);

            return docClient
                .GetDocument(new Uri(url), string.Format("Loading shows on \"{0}\"", server.Name))
                .ParseShows(server, this);
        }

        internal List<Episode> GetEpisodes(Show show)
        {
            // ShowId == GroupId
            var urlTemplate = "https://us-my.simple.tv/Library/ShowDetail?browserDateTimeUTC={0}&browserUTCOffsetMinutes={1}&groupID={2}";
            var url = string.Format(urlTemplate, BrowserDateTimeUTC, BrowserUTCOffsetMinutes, show.Id);

            return docClient
                .GetDocument(new Uri(url), string.Format("Loading episodes for \"{0}\"", show.Name))
                .ParseEpisodes(show);
        }

        public string GetEpisodeLocation(Episode episode)
        {
            // ShowId == GroupId
            var urlTemplate = "https://us-my.simple.tv/Library/Player?browserUTCOffsetMinutes={0}&groupID={1}&itemID={2}&instanceID={3}&isReachedLocally=true";
            var url = string.Format(urlTemplate, BrowserUTCOffsetMinutes, episode.Show.Id, episode.Id, episode.InstanceId);

            return docClient.GetDocument(new Uri(url), string.Format("Finding Episode \"{0}\"", episode.EpisodeName))
                .ParseEpisodeLocation();
        }

        internal void Download(string fullPathToVideo, string fileName)
        {
            var description = string.Format("Downloading {0} to {1}", fullPathToVideo, fileName);
            Console.WriteLine(description);

            var directory = Path.GetDirectoryName(fileName);
            Directory.CreateDirectory(directory);

            var syncObj = new Object();
            lock (syncObj)
            {
                webClient.DownloadFileAsync(new Uri(fullPathToVideo), fileName, syncObj);
                // This will wait until the download completes
                Monitor.Wait(syncObj);
            }
        }

        private void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            lock (e.UserState)
            {
                //releases blocked thread
                Monitor.Pulse(e.UserState);
            }
            Console.WriteLine();
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            var messageTemplate = "{0}%\t{1}Mb/{2}Mb";
            var message = string.Format(messageTemplate, e.ProgressPercentage, e.BytesReceived/1024/1024, e.TotalBytesToReceive/1024/1024);

            lock (sync)
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(message);
                Console.Write("     ");
            }
        }
    }
}
