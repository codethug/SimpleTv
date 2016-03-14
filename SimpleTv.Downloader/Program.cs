using System;

using SimpleTv.Sdk;

namespace SimpleTv.Downloader
{
    class Program
    {
        static void Main(string[] args)
        {
            // Todo: Refactor and add tests
            // Todo: Intelligently figure out what files remain to be downloaded (don't redownload if we already have it)
            //      Perhaps compare file size locally with file size on server for each file
            // Todo: After building replacement dictionary, filter out each term to avoid illegal characters
            // Todo: Verify that progress bar / percent downloading works, perhaps add 'time remaining'
            // Todo: Remove Username/Password/BaseDownloadFolder from here
            // Todo: Checkin to GitHub
            // Todo: Remove all Console.WriteLine events from SDK, add Events that are consumed by Program
            // Todo: Resume downloads that are interrupted (if supported by server)
            // Todo: Setup Plex to Auto-Scan
            // Todo: Setup Download as Scheduled Task on Plex box from Simple.Tv box

            var username = "simpletvusername";
            var password = "simpletvpassword";
            var baseDownloadFolder = "D:\\TV";
            var folderFormat = "{ShowName}\\Season {SeasonNumber00}";
            var fileNameFormat = "{ShowName} - S{SeasonNumber00}E{EpisodeNumber00} - {EpisodeName}.mp4";

            var client = new SimpleTvClient();
            if (client.Login(username, password))
            {
                foreach (var server in client.MediaServers)
                {
                    foreach (var show in server.Shows)
                    {
                        foreach (var episode in show.Episodes)
                        {
                            episode.Download(baseDownloadFolder, folderFormat, fileNameFormat);
                        }
                    }
                }

            }
            else
            {
                Console.WriteLine("Login Failed");
            }
        }
    }
}
