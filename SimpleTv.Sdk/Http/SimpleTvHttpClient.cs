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

namespace SimpleTv.Sdk.Http
{
    public class SimpleTvHttpClient
    {
        private const string simpleTvBaseUrl = "https://us.simple.tv";
        private readonly CookieAwareWebClient client;
        private IClock clock;
        Object sync;
        private IDateTimeZoneProvider dtzProvider;

        internal SimpleTvHttpClient(IClock clock, IDateTimeZoneProvider dtzProvider)
        {
            sync = new Object();
            client = new CookieAwareWebClient();
            this.clock = clock;
            this.dtzProvider = dtzProvider;
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

        internal bool Login(string un, string pw)
        {
            Console.WriteLine("Logging In");
            // Perform Login
            client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            var rawResponse = client.UploadString(
                simpleTvBaseUrl + "/Auth/SignIn",
                string.Format(
                    "InvitationKey=&UserName={0}&Password={1}&RememberMe=false&browserDateTime={2}",
                    HttpUtility.UrlEncode(un),
                    HttpUtility.UrlEncode(pw),
                    HttpUtility.UrlEncode(BrowserDateTimeUTC)));

            // Bad Login:
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
            Console.WriteLine("Loading Media Servers");
            var url = "http://us-my.simple.tv";
            var response = client.DownloadString(url);

            var html = new HtmlDocument();
            html.LoadHtml(response);

            // Get accountId from #watchShow
            //	<section id="watchShow"
            //				data-ispremium="True"
            //				data-accountid="786096d1-d810-4fbd-b0d9-ca97f779309e"
            //				data-accountguid="d079eaa7-ba31-4db2-b261-65c6d1e7fac1"
            //				data-mediaserverid="49535352-5453-3156-0005-100000001576"
            //				data-currentswversion="1064"
            //				data-mediaservergen="1"
            //				data-userid="b3e270d0-9116-4645-ae26-7e8cef76107e"
            //				data-setup="True"
            //				data-registeredtocurrentaccount="True">
            //	</section>
            var watchShowAttributes = html.GetElementbyId("watchShow").Attributes;

            //  <ul class="switch-dvr-list">
            //    <li>
            //	    <a data-value="49535352-5453-3156-0005-100000001576" class="current-dvr my-dvr">Family Room</a>
            //    </li>
            //  </ul>
            var switchDvrList = html.SelectClass("switch-dvr-list");

            var mediaServers = switchDvrList
                .SelectTag("a")
                .Select(a =>
                    new MediaServer(this)
                    {
                        Id = new Guid(a.Attributes["data-value"].Value),
                        Name = a.InnerText,
                        AccountId = new Guid(watchShowAttributes["data-accountid"].Value),
                        AccountGuid = new Guid(watchShowAttributes["data-accountguid"].Value),
                        CurrentSwVersion = Int16.Parse(watchShowAttributes["data-currentswversion"].Value),
                        MediaServerGeneration = Int16.Parse(watchShowAttributes["data-mediaservergen"].Value),
                        UserId = new Guid(watchShowAttributes["data-userid"].Value),
                        Setup = Boolean.Parse(watchShowAttributes["data-setup"].Value),
                        RegisteredToCurrentAccount = Boolean.Parse(watchShowAttributes["data-registeredtocurrentaccount"].Value)
                    }
                ).ToList();

            mediaServers.ForEach(ms =>
            {
                LocateMediaServer(ms);
                TestMediaServerLocations(ms);
            });

            return mediaServers;
        }

        private MediaServer LocateMediaServer(MediaServer server)
        {
            Console.WriteLine("Locating Media Server " + server.Name);
            var urlTemplate = "https://us-my.simple.tv/Data/RealTimeData?accountId={0}&mediaServerId={1}&playerAlternativeAvailable=false";

            var url = string.Format(urlTemplate, server.AccountId, server.Id);
            var rawResponse = client.DownloadString(url);
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

        private void TestMediaServerLocations(MediaServer ms)
        {
            Console.WriteLine("Testing Media Server Locations for " + ms.Name);
            if (PingUrl(ms.LocalPingUrl))
            {
                ms.UseLocalStream = true;
            }
            else if (PingUrl(ms.RemotePingUrl))
            {
                ms.UseLocalStream = false;
            }
            else
            {
                throw new Exception("Cannot access local or remote stream");
            }
        }

        private bool PingUrl(string url)
        {
            try
            {
                client.DownloadData(url);
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

            var response = client.DownloadString(url);
            var html = new HtmlDocument();
            html.LoadHtml(response);

            //	<section class="my-shows-list">
            //		<figure data-groupid="c868a1ab-468f-11e5-b06f-22000b688027">
            //			<div class="thumbnail-showcard">
            //				<img onerror="showAlternateImage(this, 'http://s3.amazonaws.com/simpletv/image/noimage_group_20004.png')" alt="Heroes Reborn" src="http://simp.tmsimg.com/assets/p10578128_b_h6_ab.jpg" />	
            //			</div>
            //			<figcaption>
            //				<b>Heroes Reborn</b>
            //				<span class="no">12</span> recorded <span>shows</span>
            //			</figcaption>
            //	    </figure>
            //		<figure data-groupid="0912f1a0-2a19-4cfd-879c-6b62d6567ff3">
            //		<figure data-groupid="e6df63ed-e6b5-4bec-93e6-358ed8b5e656">
            //		<figure data-groupid="a56223cb-df08-11e3-ae60-22000b278f17">

            var shows = html
                .SelectClass("my-shows-list")
                .SelectTag("figure")
                .Select(f => new Show(this, server)
                {
                    Id = new Guid(f.Attributes["data-groupid"].Value),
                    Name = f.SelectTag("b").First().InnerText,
                    NumEpisodes = Int16.Parse(f.SelectClass("no").First().InnerText)
                }).ToList();

            return shows;
        }

        internal List<Episode> GetEpisodes(Show show)
        {
            // ShowId == GroupId
            var urlTemplate = "https://us-my.simple.tv/Library/ShowDetail?browserDateTimeUTC={0}&browserUTCOffsetMinutes={1}&groupID={2}";
            var url = string.Format(urlTemplate, BrowserDateTimeUTC, BrowserUTCOffsetMinutes, show.Id);

            var response = client.DownloadString(url);
            var html = new HtmlDocument();
            html.LoadHtml(response);

            //	<div id="recorded" class="episodes">
            //        <article>
            //			<div class="f_right buttons">
            //				<a data-groupid="0912f1a0-2a19-4cfd-879c-6b62d6567ff3" data-itemid="d0bf0ce7-8a05-11e5-b973-22000b6981a6" data-instanceid="6db5790a-8940-11e5-b973-22000b6981a6" class="button-standard-watch">Watch <span></span></a>
            //				...
            //			</div>
            //			<h3>Blood Brothers <span class="show-duration">( 1 hr 45 secs )</span></h3>                    
            //			<div class="show-details-info">
            //				Tuesday, November 24 at 8:00 PM	&nbsp;·&nbsp; <b>6.1</b> WTVRDT
            //				<span class="show-attribute">NEW</span>
            //				<span class="show-attribute">TV14|TVPG</span>
            //				<span class="show-attribute">HD</span>
            //				<br />
            //				Season: <b>13</b> Episode: <b>10</b>
            //				<!--&nbsp;&nbsp;·&nbsp; 24 minutes-->
            //			</div>
            //			<p>A sailor who lost two siblings -- Marines who were killed in the line of duty -- needs a bone marrow transplant; Bishop returns to Oklahoma for Thanksgiving.</p>
            //       </article>	

            var episodes = html.GetElementbyId("recorded")
                .SelectTag("article")
                .Select(article => new Episode(this, show) {
                    Id = new Guid(article.SelectTag("a").First().Attributes["data-itemid"].Value),
                    InstanceId = new Guid(article.SelectTag("a").First().Attributes["data-instanceid"].Value),
                    EpisodeName = article.SelectTag("h3").First().ChildNodes[0].InnerText.Trim(),
                    Description = article.SelectTag("p").First().InnerText.Trim(),
                    SeasonNumber = Int16.Parse(article.SelectClass("show-details-info").SelectTag("b").Skip(1).First().InnerText.Trim()),
                    EpisodeNumber = Int16.Parse(article.SelectClass("show-details-info").SelectTag("b").Skip(2).First().InnerText.Trim())
                }).ToList();

            return episodes;
        }

        internal string GetEpisodeLocation(Episode episode)
        {
            // ShowId == GroupId
            var urlTemplate = "https://us-my.simple.tv/Library/Player?browserUTCOffsetMinutes={0}&groupID={1}&itemID={2}&instanceID={3}&isReachedLocally=true";
            var url = string.Format(urlTemplate, BrowserUTCOffsetMinutes, episode.show.Id, episode.Id, episode.InstanceId);

            var response = client.DownloadString(url);
            var html = new HtmlDocument();
            html.LoadHtml(response);

            // 	<div id="video-player-large" data-streamlocation="/7fa7fa16-9e45-47a5-a7cf-ae2c863e0e11/content/20151001T165901.91516f04-5ec8-11e5-b06f-22000b688027/tv.main.hls-0.m3u8" data-denystreaming="false" data-denystreamingmessage="Please upgrade to the Simple.TV Premier Service to watch this show remotely.">

            var fileLocation = html.GetElementbyId("video-player-large")
                .Attributes["data-streamlocation"].Value;

            // regex = /tv.main.hls-(\1\d).m3u8/;
            // url = (baseStreamUrl + streamLocation).replace(regex, "tv.4500000.10\$1")
            string pattern = @"tv\.main\.hls-(\d)\.m3u8";
            string replacement = @"tv.4500000.10$1";
            var correctedFileLocation = Regex.Replace(fileLocation, pattern, replacement);
            return correctedFileLocation;
        }

        internal void Download(Episode episode, string fileName)
        {
            client.DownloadProgressChanged += Client_DownloadProgressChanged;
            client.DownloadFileCompleted += Client_DownloadFileCompleted;

            var fullPathToVideo = episode.show.server.StreamBaseUrl + GetEpisodeLocation(episode);
            Console.WriteLine("Downloading " + fullPathToVideo + " to " + fileName);
            var directory = Path.GetDirectoryName(fileName);
            Directory.CreateDirectory(directory);

            var syncObj = new Object();
            lock (syncObj)
            {
                client.DownloadFileAsync(new Uri(fullPathToVideo), fileName, syncObj);
                //This would block the thread until download completes
                Monitor.Wait(syncObj);
            }

            client.DownloadFileCompleted -= Client_DownloadFileCompleted;
            client.DownloadProgressChanged -= Client_DownloadProgressChanged;
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
