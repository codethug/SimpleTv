using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTv.Sdk
{
    public class Configuration
    {
        public string Username { get; set; }
        public string Password { get; set; }
        // Use newer Folder parameter instead of DownloadFolder.  
        public string DownloadFolder { get; set; }
        public string FolderFormat { get; set; }
        public string FilenameFormat { get; set; }
        public string ShowFilter { get; set; }
        public bool LogHttpCalls { get; set; }
    }
}
