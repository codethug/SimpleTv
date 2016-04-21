using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTv.Sdk.Diagnostics
{
    public class StreamNotFoundException : Exception
    {
        public StreamNotFoundException() : base() { }
        public StreamNotFoundException(string message) : base(message) { }
    }
}
