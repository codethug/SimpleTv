using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTv.Sdk.Diagnostics
{
    public class HttpResponseReceivedEventArgs : EventArgs
    {
        public HttpData HttpData { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
