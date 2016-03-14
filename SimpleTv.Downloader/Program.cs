using System;

using SimpleTv.Sdk;

namespace SimpleTv.Downloader
{
    class Program
    {
        static void Main(string[] args)
        {
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
