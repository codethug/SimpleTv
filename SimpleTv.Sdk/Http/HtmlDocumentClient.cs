using HtmlAgilityPack;
using SimpleTv.Sdk.Diagnostics;
using SimpleTv.Sdk.Http;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SimpleTv.Sdk.Http
{
    public class HtmlDocumentClient : IHtmlDocumentClient
    {
        private IWebClient webClient;
        public HtmlDocumentClient(IWebClient client)
        {
            this.webClient = client;
        }

        public event EventHandler<HttpResponseReceivedEventArgs> HttpResponseReceived;
        protected virtual void OnHttpResponseReceived(HttpData data)
        {
            if (HttpResponseReceived != null)
            {
                var eventArgs = new HttpResponseReceivedEventArgs()
                {
                    HttpData = data,
                    Timestamp = DateTime.UtcNow
                };
                HttpResponseReceived(this, eventArgs);
            }
        }

        public string GetRaw(Uri uri, string description = "")
        {
            Console.WriteLine(description);
            var rawResponse = webClient.DownloadString(uri);

            OnHttpResponseReceived(new HttpData()
            {
                Description = description,
                RequestedUrl = uri.ToString(),
                HttpVerb = "GET",
                Response = rawResponse
            });

            return rawResponse;
        }

        public HtmlDocument GetDocument(Uri uri, string description = "")
        {
            return GetRaw(uri, description).AsHtmlDocument();
        }

        public string FormatQueryString(NameValueCollection data)
        {
            if (data == null)
            {
                return String.Empty;
            }

            return String.Join("&",
                data.AllKeys
                    .Select(key => string.Format("{0}={1}",
                       HttpUtility.UrlEncode(key),
                       HttpUtility.UrlEncode(data[key])
                    )));
        }

        public string PostRawReponse(Uri uri, string description, NameValueCollection data)
        {
            // Todo: don't have this as embedded dependency
            Console.WriteLine(description);

            webClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

            var rawResponse = webClient.UploadString(uri, FormatQueryString(data));

            OnHttpResponseReceived(new HttpData()
            {
                Description = description,
                RequestedUrl = uri.ToString(),
                HttpVerb = "POST",
                Response = rawResponse
            });

            return rawResponse;
        }
    }
}
