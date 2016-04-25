using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTv.Sdk.Models
{
    public class DownloadDetails
    {
        public Episode Episode { get; set; }
        public bool FileExistsWithSameSize { get; set; }
        public Uri FullUriToVideo { get; set; }
        public long DownloadSize { get;  set;}
        public string FilePathAndName { get; set; }
    }
}
