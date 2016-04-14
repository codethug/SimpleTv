using HtmlAgilityPack;
using SimpleTv.Sdk.Diagnostics;
using SimpleTv.Sdk.Models;
using SimpleTv.Sdk.Naming;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SimpleTv.Sdk.Http
{
    public static class HtmlParserExtensions
    {
        public static List<MediaServer> ParseMediaServers(this HtmlDocument html, SimpleTvHttpClient client)
        {
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
            return html
                .SelectClass("switch-dvr-list")
                .SelectTag("a")
                .Select(a =>
                    new MediaServer()
                    {
                        Id = new Guid(a.Attributes["data-value"].Value),
                        Name = a.InnerText,
                        AccountId = new Guid(watchShowAttributes["data-accountid"].Value),
                        AccountGuid = new Guid(watchShowAttributes["data-accountguid"].Value),
                        CurrentSwVersion = watchShowAttributes["data-currentswversion"].Value,
                        MediaServerGeneration = Int32.Parse(watchShowAttributes["data-mediaservergen"].Value),
                        UserId = new Guid(watchShowAttributes["data-userid"].Value),
                        Setup = Boolean.Parse(watchShowAttributes["data-setup"].Value),
                        RegisteredToCurrentAccount = Boolean.Parse(watchShowAttributes["data-registeredtocurrentaccount"].Value)
                    }
                ).ToList();
        }

        public static List<Show> ParseShows(this HtmlDocument html, MediaServer server, SimpleTvHttpClient client)
        {
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
            return html.SelectClass("my-shows-list")
                .SelectTag("figure")
                .Select(f => new Show(server)
                {
                    Id = new Guid(f.Attributes["data-groupid"].Value),
                    Name = f.SelectTag("b").First().InnerText,
                    NumEpisodes = Int32.Parse(f.SelectClass("no").First().InnerText)
                }).ToList();
        }

        /// <summary>
        /// Finds substring of string starting at startIndex and ending just before
        /// the first instance of termination
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="termination"></param>
        /// <returns></returns>
        private static string Substring(this string input, int startIndex, string termination) {
            var terminationIndex = input.IndexOf(termination, startIndex);
            if (terminationIndex == -1)
            {
                return input.Substring(startIndex);
            }
            else
            {
                return input.Substring(startIndex, terminationIndex);
            }
        }


        /// <summary>
        /// Parses a date time string from Simple.Tv.
        /// The difficulty is that we're not sure what the year is.  We currently assume
        /// that it is for the current year as long as the date provided happened earlier in
        /// the year than today's date.
        /// 
        /// This function will return incorrect data for any recordings made more than 1 year ago
        /// </summary>
        /// <param name="input"></param>
        /// <returns>DateTime object</returns>
        /// <example>Input: Thursday, September 24 at 8:00 PM</example>
        private static DateTime? ParseAsDateTimeWithoutYear(this string input)
        {
            // First, strip out the day of week
            var inputNoDayOfWeek = input.Substring(input.IndexOf(",") + 2);
            // Next, create the date object (defaults to this year)
            DateTime parsedDate;
            if (!DateTime.TryParseExact(inputNoDayOfWeek, "MMMM d 'at' h:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
            {
                return null;
            }

            if (parsedDate > DateTime.Now)
            {
                return parsedDate.AddYears(-1);
            }
            else
            {
                return parsedDate;
            }
        }

        public static List<Episode> ParseEpisodes(this HtmlDocument html, Show show)
        {
            return html.GetElementbyId("recorded")
                .SelectTag("article")
                .Select(article => new Episode(show)
                {
                    Id = article.SelectTag("a").FirstOrDefault()
                        .IfNotNull(e => new Guid(e.Attributes["data-itemid"].Value)),
                    InstanceId = article.SelectTag("a").FirstOrDefault()
                        .IfNotNull(e => new Guid(e.Attributes["data-instanceid"].Value)),
                    EpisodeName = article.SelectTag("h3").FirstOrDefault()
                        .IfNotNull(e => e.ChildNodes[0].InnerText.Trim()),
                    Description = article.SelectTag("p").FirstOrDefault()
                        .IfNotNull(e => e.InnerText.Trim()),
                    SeasonNumber = article.SelectClass("show-details-info").SelectTag("b").Skip(1).FirstOrDefault()
                        .IfNotNull(e => Int32.Parse(e.InnerText.Trim())),
                    EpisodeNumber = article.SelectClass("show-details-info").SelectTag("b").Skip(2).FirstOrDefault()
                        .IfNotNull(e => Int32.Parse(e.InnerText.Trim())),

                    ChannelNumber = article.SelectClass("show-details-info").SelectTag("b").FirstOrDefault()
                        .IfNotNull(e => e.InnerText.Trim()),
                    DateTime = article.SelectClass("show-details-info").FirstOrDefault()
                        .IfNotNull(e => e
                            .InnerHtml
                            .Substring(0,"&nbsp;").Trim()
                            .ParseAsDateTimeWithoutYear()
                        )
                }).ToList();
        }

        private static bool ShouldDenyStreaming(this HtmlNode vidPlayer)
        {
            // 	<div id="video-player-large" 
            //       data-streamlocation="/7fa7fa16-9e45-47a5-a7cf-ae2c863e0e11/content/20151001T165901.91516f04-5ec8-11e5-b06f-22000b688027/tv.main.hls-0.m3u8" 
            //       data-denystreaming="false" 
            //       data-denystreamingmessage="Please upgrade to the Simple.TV Premier Service to watch this show remotely.">

            bool shouldDeny = false;

            var denyStreamingAttribute = vidPlayer.Attributes["data-denystreaming"];
            if (denyStreamingAttribute != null && !string.IsNullOrWhiteSpace(denyStreamingAttribute.Value))
            {
                bool.TryParse(denyStreamingAttribute.Value, out shouldDeny);
            }

            return shouldDeny;
        }

        public static string ParseEpisodeLocation(this HtmlDocument html)
        {
            // 	<div id="video-player-large" 
            //       data-streamlocation="/7fa7fa16-9e45-47a5-a7cf-ae2c863e0e11/content/20151001T165901.91516f04-5ec8-11e5-b06f-22000b688027/tv.main.hls-0.m3u8" 
            //       data-denystreaming="false" 
            //       data-denystreamingmessage="Please upgrade to the Simple.TV Premier Service to watch this show remotely.">
            var vidPlayer = html.GetElementbyId("video-player-large");

            if (vidPlayer.ShouldDenyStreaming())
            {
                var message = "Please upgrade to the Simple.TV Premier Service to download this show remotely.";
                throw new DenyStreamingException(message);
            }

            return vidPlayer
                .Attributes["data-streamlocation"].Value

                // regex = /tv.main.hls-(\1\d).m3u8/;
                // url = (baseStreamUrl + streamLocation).replace(regex, "tv.4500000.10\$1")
                .RegexReplace(@"tv\.main\.hls-(\d)\.m3u8", @"tv.4500000.10$1");
        }

        public static T IfNotNull<S,T>(this S source, Func<S,T> evaluateIfSourceNotNull)
        {
            if (source == null)
            {
                return default(T);
            } else
            {
                return evaluateIfSourceNotNull(source);
            }
        }
    }
}
