using System.Collections.Generic;

using NodaTime;

using SimpleTv.Sdk.Models;
using SimpleTv.Sdk.Http;
using SimpleTv.Sdk.Diagnostics;
using System;
using SimpleTv.Sdk.Naming;
using System.Linq;

namespace SimpleTv.Sdk
{
    public class SimpleTvClient
    {
        private ISimpleTvHttpClient tvHttpClient;

        public SimpleTvClient(ISimpleTvHttpClient tvHttpClient)
        {
            this.tvHttpClient = tvHttpClient;
        }

        public bool Login(string username, string password)
        {
            return tvHttpClient.Login(username, password);
        }

        public IEnumerable<MediaServer> GetMediaServers(string include = "*", string exclude = "")
        {
            return tvHttpClient.GetMediaServers()
                .IncludeOnly(include)
                .Exclude(exclude)
                .Where(ms =>
                {
                    // Find out where the DVR is on the internet/network
                    tvHttpClient.LocateMediaServer(ms);
                    // Ensure we can actually communicate with the DVR
                    return tvHttpClient.TestMediaServerLocations(ms);
                });
        }

        public IEnumerable<Show> GetShows(MediaServer server, string include, string exclude)
        {
            return tvHttpClient.GetShows(server)
                .IncludeOnly(include)
                .Exclude(exclude);
        }

        public List<Episode> GetEpisodes(Show show)
        {
            return tvHttpClient.GetEpisodes(show);
        }

        public void DownloadEpisode(Episode episode, string downloadFolder, string folderFormat, string filenameFormat)
        {
            var fileName = episode.GenerateFileName(downloadFolder, folderFormat, filenameFormat);
            var fullPathToVideo = new Uri(episode.Show.Server.StreamBaseUrl + tvHttpClient.GetEpisodeLocation(episode));

            if (tvHttpClient.IsBigEnoughToDownload(fullPathToVideo, 1024*1024, episode.EpisodeName))
            {
                tvHttpClient.Download(fullPathToVideo, fileName, episode.EpisodeName);
            }
        }
    }
}
