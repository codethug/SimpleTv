﻿using System;
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
    public class SimpleTvHttpClient : ISimpleTvHttpClient
    {
        private const string simpleTvBaseUrl = "https://us.simple.tv";
        private readonly IWebClient webClient;
        private readonly IHtmlDocumentClient docClient;
        private IClock clock;
        Object sync;
        private IDateTimeZoneProvider dtzProvider;
        private string username;

        public SimpleTvHttpClient(IClock clock, IDateTimeZoneProvider dtzProvider, IWebClient webClient, IHtmlDocumentClient docClient)
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
                    .ToString("yyyy/M/d H:m:s");
            }
        }

        // POST to https://us.simple.tv/Auth/SignIn
        //   ?InvitationKey=&UserName=user%40gmail.com&Password=[EncodedPW]&RememberMe=false&browserDateTime=2016/3/11 19:28:16
        // Keep cookies for future requests
        public bool Login(string un, string pw)
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
                this.username = null;
                return false;
            }
            this.username = un;
            return true;
        }

        public List<MediaServer> GetMediaServers()
        {
            return docClient.GetDocument(new Uri("http://us-my.simple.tv"), "Loading Media Servers")
                .ParseMediaServers(this);
        }

        public MediaServer LocateMediaServer(MediaServer server)
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

        public bool TestMediaServerLocations(MediaServer ms)
        {
            var description = string.Format("Testing Media Server \"{0}\"", ms.Name);
            if (PingUrl(ms.LocalPingUrl, description))
            {
                ms.UseLocalStream = true;
                return true;
            }
            else if (PingUrl(ms.RemotePingUrl, description))
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

        private bool PingUrl(string url, string description)
        {
            try
            {
                docClient.GetRaw(new Uri(url), description);
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

        private MediaServer currentMediaServer;
        private void SetMediaServer(MediaServer server)
        {
            if (currentMediaServer != server)
            {
                var uri = new Uri("https://us-my.simple.tv/Account/MediaServers");

                docClient.PostRawReponse(uri, "Setting Media Server to \"" + server.Name + "\"", new NameValueCollection {
                    { "defaultMediaServerID", server.Id.ToString() }
                });

                currentMediaServer = server;
            };
        }

        public void Reboot(MediaServer server)
        {
            var uri = new Uri("https://us-my.simple.tv/Setup/RebootMediaServer");
            docClient.PostRawReponse(uri, "Rebooting Media Server \"" + server.Name + "\"", new NameValueCollection {
                    { "mediaServerID", server.Id.ToString() }
            });
        }

        public List<Show> GetShows(MediaServer server)
        {
            SetMediaServer(server);

            var urlTemplate = "https://us-my.simple.tv/Library/MyShows?browserDateTimeUTC={0}&mediaServerID={1}&browserUTCOffsetMinutes={2}";
            var url = string.Format(urlTemplate, BrowserDateTimeUTC, server.Id, BrowserUTCOffsetMinutes);

            return docClient
                .GetDocument(new Uri(url), string.Format("Loading shows on \"{0}\"", server.Name))
                .ParseShows(server, this);
        }

        public List<Episode> GetEpisodes(Show show)
        {
            SetMediaServer(show.Server);

            // ShowId == GroupId
            var urlTemplate = "https://us-my.simple.tv/Library/ShowDetail?browserDateTimeUTC={0}&browserUTCOffsetMinutes={1}&groupID={2}";
            var uri = new Uri(string.Format(urlTemplate, HttpUtility.UrlEncode(BrowserDateTimeUTC).ToUpper(), BrowserUTCOffsetMinutes, show.Id));

            return docClient
                //.GetDocumentAjax(uri, show.Server.AccountId, string.Format("Loading episodes for \"{0}\"", show.Name))
                .GetDocument(uri, string.Format("Loading episodes for \"{0}\"", show.Name))
                .ParseEpisodes(show)
                .ToList();
        }

        public string GetEpisodeLocation(Episode episode)
        {
            SetMediaServer(episode.Show.Server);

            // ShowId == GroupId
            var urlTemplate = "https://us-my.simple.tv/Library/Player?browserUTCOffsetMinutes={0}&groupID={1}&itemID={2}&instanceID={3}&isReachedLocally=true";
            var url = string.Format(urlTemplate, BrowserUTCOffsetMinutes, episode.Show.Id, episode.Id, episode.InstanceId);

            var message = string.Format("Finding Episode \"{0}\" from {1}",
                    episode.EpisodeName,
                    episode.DateTime.IfNotNull(d => d.Value.ToString("d"))
                );

            return docClient.GetDocument(new Uri(url), message)
                .ParseEpisodeLocation();
        }

        public void Download(Uri fullPathToVideo, string fileName, string episodeName)
        {
            var syncObj = new Object();
            lock (syncObj)
            {
                var description = string.Format("Downloading \"{0}\" to {1}", episodeName, fileName);
                Console.WriteLine(description);

                var directory = Path.GetDirectoryName(fileName);
                Directory.CreateDirectory(directory);

                webClient.DownloadFileAsync(fullPathToVideo, fileName, syncObj);
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
            var messageTemplate = "{0}%\t{1}/{2}MB";
            var message = string.Format(messageTemplate, e.ProgressPercentage, e.BytesReceived/1024/1024, e.TotalBytesToReceive/1024/1024);

            lock (sync)
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(message);
                Console.Write("     ");
            }
        }

        public long GetFileSize(Uri uri)
        {
            return webClient.GetFileSize(uri);
        }
    }
}
