using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTv.Sdk.Diagnostics
{
    public class HttpData
    {
        public string Description { get; set; }
        public string RequestedUrl { get; set; }
        public string HttpVerb { get; set; }
        public string Response { get; set; }
    }
}
