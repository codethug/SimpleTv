using System;

using Fclp;
using System.IO;
using SimpleTv.Sdk.Diagnostics;
using System.Reflection;
using SimpleTv.Sdk;

namespace SimpleTv.Downloader
{
    class Program
    {
        private static string execName = Path.GetFileName(System.Reflection.Assembly.GetEntryAssembly().Location);

        static void Main(string[] args)
        {
            var p = SetupArguments();
            var result = p.Parse(args);
            if (result.HasErrors)
            {
                p.HelpOption.ShowHelp(p.Options);
            }
            else
            {
                string error = null;
                var downloader = new Downloader(p.Object);
                try
                {
                    if (p.Object.Reboot)
                    {
                        downloader.Reboot();
                    }
                    else
                    {
                        downloader.Download();
                    }
                }
                catch (Exception e)
                {
                    error = e.AsDetailedString();
                    Console.WriteLine(error);
                    if (!p.Object.LogHttpCalls)
                    {
                        Console.WriteLine();
                        Console.WriteLine("To log details about the HTTP calls, run this again with the '-l' flag");
                        Console.WriteLine();
                    }
                }
                finally
                {
                    if (p.Object.LogHttpCalls)
                    {
                        downloader.SaveHttpLogs(error);
                    }
                }
            }
        }

        public static FluentCommandLineParser<Configuration> SetupArguments()
        {
            var p = new FluentCommandLineParser<Configuration>();

            p.Setup(arg => arg.Username)
                .As('u', "username")
                .WithDescription("Username for logging into Simple.Tv")
                .Required();

            p.Setup(arg => arg.Password)
                .As('p', "password")
                .WithDescription("Password for logging into Simple.Tv")
                .Required();

            // Later on, -d / -dryrun will be used to do a dry run (parsing without downloading)

            p.Setup(arg => arg.ShowIncludeFilter)
                .As('i', "includeShows")
                .WithDescription("[optional] Type in show(s) to include.  Wildcards accepted.")
                .SetDefault("*");

            p.Setup(arg => arg.ShowExcludeFilter)
                .As('x', "excludeShows")
                .WithDescription("[optional] Type in show(s) to exclude.  Wildcards accepted.")
                .SetDefault(string.Empty);

            p.Setup(arg => arg.ServerIncludeFilter)
                .As('s', "includeServers")
                .WithDescription("[optional] Type in media server(s) to include.  Wildcards accepted.")
                .SetDefault("*");

            p.Setup(arg => arg.ServerExcludeFilter)
                .As('e', "excludeServers")
                .WithDescription("[optional] Type in media server(s) to exclude.  Wildcards accepted.")
                .SetDefault(string.Empty);

            p.Setup(arg => arg.DownloadFolder)
                .As('f', "downloadFolder")
                .WithDescription("[optional] Folder to place downloaded recordings in.  Defaults to current working directory.")
                .SetDefault(Directory.GetCurrentDirectory());

            p.Setup(arg => arg.FolderFormat)
                .As('t', "folderformat")
                .WithDescription("[optional] The folder format for saving the recording, relative to the downloadfolder.  Defaults to Plex format defined at https://support.plex.tv/hc/en-us/articles/200220687-Naming-Series-Season-Based-TV-Shows")
                .SetDefault("{ShowName}\\Season {SeasonNumber00}");

            p.Setup(arg => arg.FilenameFormat)
                .As('n', "filenameformat")
                .WithDescription("[optional] The filename format for saving the recording.  Defaults to Plex format defined at https://support.plex.tv/hc/en-us/articles/200220687-Naming-Series-Season-Based-TV-Shows")
                .SetDefault("{ShowName} - S{SeasonNumber00}E{EpisodeNumber00} - {EpisodeName}.mp4");

            p.Setup(arg => arg.LogHttpCalls)
                .As('l', "logHttpCalls")
                .WithDescription("[optional] Will save a log of all http calls, helpful for debugging errors")
                .SetDefault(false);

            p.Setup(arg => arg.Reboot)
                .As('r', "reboot")
                .WithDescription("Used to reboot Simple.TV DVRs.  Can be used with -s and -t.")
                .SetDefault(false);

            p.SetupHelp("?", "help")
                .WithHeader(string.Format(
                    "SimpleTV Downloader\n" + 
                    "Version {0}\n" +
                    "\n" +
                    "SimpleTV Downloader can download your Simple.Tv recordings.  \r\n" +
                    "Usage:\r\n" +
                    "\t{1} -u username@somewhere.com -p \"P@ssw0Rd\" -d c:\\tvshows -i \"NCIS*\"",
                    Assembly.GetExecutingAssembly().GetName().Version, execName
                ))
                .Callback(text => Console.WriteLine(text));

            return p;
        }

    }
}
