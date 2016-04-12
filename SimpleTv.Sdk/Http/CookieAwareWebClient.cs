using System;
using System.Net;

namespace SimpleTv.Sdk.Http
{
    // CookieAwareWebClient from http://stackoverflow.com/questions/31129578/using-cookie-aware-webclient
    internal class CookieAwareWebClient : WebClient, IWebClient
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
    }
}
