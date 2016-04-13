using System;

using Fclp;
using System.IO;
using SimpleTv.Sdk.Diagnostics;
using System.Reflection;

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
                if (p.Object.DownloadFolder != null)
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("The -d / -downloadfolder parameter has been renamed.  Please use -f / -folder instead.");
                    Console.ResetColor();
                }
                else
                {

                    string error = null;
                    var downloader = new Downloader(p.Object);
                    try
                    {
                        downloader.Download();
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
        }

        public static FluentCommandLineParser<ApplicationArguments> SetupArguments()
        {
            var p = new FluentCommandLineParser<ApplicationArguments>();

            p.Setup(arg => arg.Username)
                .As('u', "username")
                .WithDescription("Username for logging into Simple.Tv")
                .Required();

            p.Setup(arg => arg.Password)
                .As('p', "password")
                .WithDescription("Password for logging into Simple.Tv")
                .Required();

            // Later on, -d / -dryrun will be used to do a dry run (parsing without downloading)
            p.Setup(arg => arg.DownloadFolder)
                .As('d', "downloadfolder")
                .WithDescription("[unsupported] Please use the f / folder parameter instead of d / downloadfolder.");

            p.Setup(arg => arg.Folder)
                .As('f', "folder")
                .WithDescription("[optional] Folder to place downloaded recordings in.  Defaults to current working directory.")
                .SetDefault(Directory.GetCurrentDirectory());

            p.Setup(arg => arg.ShowFilter)
                .As('s', "showFilter")
                .WithDescription("[optional] Type in which show you want to download.  Wildcards accepted.")
                .SetDefault("*");

            p.Setup(arg => arg.FolderFormat)
                .As('r', "folderformat")
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

            p.SetupHelp("?", "help")
                .WithHeader(string.Format(
                    "SimpleTV Downloader\n" + 
                    "Version {0}\n" +
                    "\n" +
                    "SimpleTV Downloader can download your Simple.Tv recordings.  \r\n" +
                    "Usage:\r\n" +
                    "\t{1} -u username@somewhere.com -p \"P@ssw0Rd\" -d c:\\tvshows -s \"NCIS*\"",
                    Assembly.GetExecutingAssembly().GetName().Version, execName
                ))
                .Callback(text => Console.WriteLine(text));

            return p;
        }

    }
}
