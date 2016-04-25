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
        static void Main(string[] args)
        {
            var p = Arguments.Setup();
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
    }
}
