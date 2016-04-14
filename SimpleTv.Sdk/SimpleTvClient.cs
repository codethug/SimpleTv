using System.Collections.Generic;

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

        public void DownloadEpisode(Episode episode)
        {
            var fileName = episode.GenerateFileName(config.DownloadFolder, config.FolderFormat, config.FilenameFormat);
            var fullPathToVideo = episode.show.server.StreamBaseUrl + _client.GetEpisodeLocation(episode); ;

            _client.Download(fullPathToVideo, fileName);
        }

    }
}
