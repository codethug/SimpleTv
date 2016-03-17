using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using SimpleTv.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTv.Downloader
{
    public class Downloader
    {
        private ApplicationArguments arguments;

        public Downloader(ApplicationArguments arguments)
        {
            this.arguments = arguments;
        }

        public void Download()
        {
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
