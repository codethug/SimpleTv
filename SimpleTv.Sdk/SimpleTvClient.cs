﻿using System.Collections.Generic;

using NodaTime;

using SimpleTv.Sdk.Models;
using SimpleTv.Sdk.Http;
using SimpleTv.Sdk.Diagnostics;
using System;
using SimpleTv.Sdk.Naming;

namespace SimpleTv.Sdk
{
    public class SimpleTvClient
    {
        private SimpleTvHttpClient _client;
        private Configuration config;

        public SimpleTvClient(Configuration config)
        {
            this.config = config;
            var webClient = new CookieAwareWebClient();
            var docClient = new HtmlDocumentClient(webClient);
            _client = new SimpleTvHttpClient(SystemClock.Instance, DateTimeZoneProviders.Bcl, webClient, docClient);
        }

        public event EventHandler<HttpResponseReceivedEventArgs> HttpResponseReceived
        {
            add { _client.HttpResponseReceived += value; }
            remove { _client.HttpResponseReceived -= value; }
        }

        public bool Login(string username, string password)
        {
            return _client.Login(username, password);
        }

        private List<MediaServer> _mediaServers;
        public List<MediaServer> MediaServers
        {
            get
            {
                if (_mediaServers == null)
                {
                    _mediaServers = _client.GetMediaServers();
                }
                return _mediaServers;
            }
        }

        public List<Show> GetShows(MediaServer server)
        {
            return _client.GetShows(server);
        }

        public List<Episode> GetEpisodes(Show show)
        {
            return _client.GetEpisodes(show);
        }

        public void DownloadEpisode(Episode episode)
        {
            var fileName = episode.GenerateFileName(config.DownloadFolder, config.FolderFormat, config.FilenameFormat);
            var fullPathToVideo = new Uri(episode.Show.Server.StreamBaseUrl + _client.GetEpisodeLocation(episode));

            if (_client.IsBigEnoughToDownload(fullPathToVideo, 1024*1024, episode.EpisodeName))
            {
                _client.Download(fullPathToVideo, fileName, episode.EpisodeName);
            }
        }
    }
}
