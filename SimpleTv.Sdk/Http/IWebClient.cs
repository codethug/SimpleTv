using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTv.Sdk.Http
{
    public interface IWebClient
    {
        string DownloadString(Uri address);
        string UploadString(Uri address, string data);
        WebHeaderCollection Headers { get; }
        event DownloadProgressChangedEventHandler DownloadProgressChanged;
        event AsyncCompletedEventHandler DownloadFileCompleted;
        void DownloadFileAsync(Uri address, string fileName, object userToken);

        long GetFileSize(Uri address);
    }
}
