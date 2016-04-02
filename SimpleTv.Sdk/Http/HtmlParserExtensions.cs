using HtmlAgilityPack;
using SimpleTv.Sdk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                        .IfNotNull(e => Int32.Parse(e.InnerText.Trim()))
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
