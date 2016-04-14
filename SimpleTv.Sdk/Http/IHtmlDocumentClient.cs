using HtmlAgilityPack;
using SimpleTv.Sdk.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTv.Sdk.Http
{
    public interface IHtmlDocumentClient
    {
        event EventHandler<HttpResponseReceivedEventArgs> HttpResponseReceived;
        string GetRaw(Uri uri, string description = "");
        HtmlDocument GetDocument(Uri uri, string description = "");
        string FormatQueryString(NameValueCollection data);
        string PostRawReponse(Uri uri, string description, NameValueCollection data);
    }
}
