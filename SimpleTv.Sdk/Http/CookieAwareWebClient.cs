using System;
using System.IO;
using System.Net;

namespace SimpleTv.Sdk.Http
{
    // CookieAwareWebClient from http://stackoverflow.com/questions/31129578/using-cookie-aware-webclient
    public class CookieAwareWebClient : WebClient, IWebClient
    {
        public CookieAwareWebClient() : base()
        {
            CookieContainer = new CookieContainer();
        }
        public CookieContainer CookieContainer { get; set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = (HttpWebRequest)base.GetWebRequest(address);
            request.Timeout = 5000;
            if (request != null)
            {
                request.CookieContainer = CookieContainer;
            }
            return request;
        }

        public long GetFileSize(Uri address)
        {
            using (var s = OpenRead(address))
            {
                return long.Parse(ResponseHeaders["Content-Length"].ToString());
            }
        }
    }
}
