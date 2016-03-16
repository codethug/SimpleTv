using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTv.Downloader
{
    public class ApplicationArguments
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string DownloadFolder { get; set; }
        public string FolderFormat { get; set; }
        public string FilenameFormat { get; set; }
        public string ShowFilter { get; set; }
    }
}
