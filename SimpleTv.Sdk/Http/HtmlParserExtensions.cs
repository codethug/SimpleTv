﻿using HtmlAgilityPack;
using SimpleTv.Sdk.Models;
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
            var switchDvrList = html.SelectClass("switch-dvr-list");

            var mediaServers = switchDvrList
                .SelectTag("a")
                .Select(a =>
                    new MediaServer(client)
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

            return mediaServers;
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

        public static List<Episode> ParseEpisodes(this HtmlDocument html, Show show, SimpleTvHttpClient client)
        {
            return html.GetElementbyId("recorded")
                .SelectTag("article")
                .Select(article => new Episode(client, show)
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
