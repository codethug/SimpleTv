using System;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

using SimpleTv.Sdk;
using Fclp;
using System.IO;
using System.Linq;

namespace SimpleTv.Downloader
{
    class Program
    {
        static void Main(string[] args)
        {
            var execName = Path.GetFileName(System.Reflection.Assembly.GetEntryAssembly().Location);
            var p = new FluentCommandLineParser<ApplicationArguments>();

            p.Setup(arg => arg.Username)
                .As('u', "username")
                .WithDescription("Username for logging into Simple.Tv")
                .Required();

            p.Setup(arg => arg.Password)
                .As('p', "password")
                .WithDescription("Password for logging into Simple.Tv")
                .Required();

            p.Setup(arg => arg.DownloadFolder)
                .As('d', "downloadfolder")
                .WithDescription("Folder to place downloaded recordings in.  Defaults to current working directory.")
                .SetDefault(Directory.GetCurrentDirectory());

            p.Setup(arg => arg.ShowFilter)
                .As('s', "showFilter")
                .WithDescription("Type in which show you want to download.  Wildcards accepted.  Will download all shows if this parameter is missing.")
                .SetDefault("*");

            p.Setup(arg => arg.FolderFormat)
                .As('r', "folderformat")
                .WithDescription("The folder format for saving the recording, relative to the downloadfolder.  Defaults to Plex format defined at https://support.plex.tv/hc/en-us/articles/200220687-Naming-Series-Season-Based-TV-Shows")
                .SetDefault("{ShowName}\\Season {SeasonNumber00}");

            p.Setup(arg => arg.FilenameFormat)
                .As('n', "filenameformat")
                .WithDescription("The filename format for saving the recording.  Defaults to Plex format defined at https://support.plex.tv/hc/en-us/articles/200220687-Naming-Series-Season-Based-TV-Shows")
                .SetDefault("{ShowName} - S{SeasonNumber00}E{EpisodeNumber00} - {EpisodeName}.mp4");

            p.SetupHelp("?", "help")
                .WithHeader(execName + " is used to download your Simple.Tv recordings.  \r\nUsage:\r\n" +
                    "\t" + execName + " -u username@somewhere.com -p \"P@ssw0Rd\" -d c:\\tvshows -s \"NCIS*\""
                )
                .Callback(text => Console.WriteLine(text));

            var result = p.Parse(args);
            if (result.HasErrors)
            {
                p.HelpOption.ShowHelp(p.Options);
            }
            else
            {
                var arguments = p.Object;
                var client = new SimpleTvClient();
                Console.WriteLine();
                if (client.Login(arguments.Username, arguments.Password))
                {
                    foreach (var server in client.MediaServers)
                    {
                        var filteredShows = server.Shows.Where(s => 
                            Operators.LikeString(s.Name, arguments.ShowFilter, CompareMethod.Text)
                        );

                        Console.WriteLine();
                        foreach (var show in filteredShows)
                        {
                            Console.WriteLine("Downloading " + show.Name);
                            Console.WriteLine("=======================================================");

                            foreach (var episode in show.Episodes)
                            {
                                episode.Download(arguments.DownloadFolder, arguments.FolderFormat, arguments.FilenameFormat);
                            }

                            Console.WriteLine("=======================================================");
                            Console.WriteLine("Finished downloading " + show.Name);
                            Console.WriteLine();
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
}
