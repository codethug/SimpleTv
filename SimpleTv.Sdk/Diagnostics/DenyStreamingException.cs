using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTv.Sdk.Diagnostics
{
    public class DenyStreamingException : Exception
    {
        public DenyStreamingException() : base() { }
        public DenyStreamingException(string message) : base(message) { }
    }
}
