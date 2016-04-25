using System.Collections.Generic;

using NodaTime;

using SimpleTv.Sdk.Models;
using SimpleTv.Sdk.Http;
using SimpleTv.Sdk.Diagnostics;
using System;
using System.IO;
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
                .Exclude(exclude);
        }

        public IEnumerable<MediaServer> FindMediaServer(IEnumerable<MediaServer> servers)
        {
            return servers
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

        public void Reboot(MediaServer server)
        {
            tvHttpClient.Reboot(server);
        }

        public DownloadDetails PrepareDownload(Episode episode, string baseFolder, string folderFormat, string fileNameFormat)
        {
            var downloadDetails = new DownloadDetails();
            downloadDetails.Episode = episode;
            downloadDetails.FilePathAndName = episode.GenerateFileName(baseFolder, folderFormat, fileNameFormat);
            downloadDetails.FullUriToVideo = new Uri(episode.Show.Server.StreamBaseUrl + tvHttpClient.GetEpisodeLocation(episode));
            downloadDetails.DownloadSize = tvHttpClient.GetFileSize(downloadDetails.FullUriToVideo);
            downloadDetails.FileExistsWithSameSize = File.Exists(downloadDetails.FilePathAndName) && (new FileInfo(downloadDetails.FilePathAndName)).Length == downloadDetails.DownloadSize;

            return downloadDetails;
        }

        public void DownloadEpisode(DownloadDetails downloadDetails)
        {
            tvHttpClient.Download(downloadDetails.FullUriToVideo, downloadDetails.FilePathAndName, downloadDetails.Episode.EpisodeName);
        }
    }
}
