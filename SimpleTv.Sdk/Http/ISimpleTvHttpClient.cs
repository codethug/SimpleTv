using SimpleTv.Sdk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTv.Sdk.Http
{
    public interface ISimpleTvHttpClient
    {
        bool Login(string un, string pw);
        List<MediaServer> GetMediaServers();
        MediaServer LocateMediaServer(MediaServer server);
        void Reboot(MediaServer server);
        bool TestMediaServerLocations(MediaServer ms);
        List<Show> GetShows(MediaServer server);
        List<Episode> GetEpisodes(Show show);
        string GetEpisodeLocation(Episode episode);
        bool IsBigEnoughToDownload(Uri uri, int sizeInBytes, string episodeName);
        void Download(Uri fullPathToVideo, string fileName, string episodeName);
    }
}
