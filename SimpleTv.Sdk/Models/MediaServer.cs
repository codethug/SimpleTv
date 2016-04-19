using System;
using System.Collections.Generic;

using SimpleTv.Sdk.Http;

namespace SimpleTv.Sdk.Models
{
    public class MediaServer : INamed
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid AccountId { get; set; }

        public Guid AccountGuid { get; set; }
        public string CurrentSwVersion { get; set; }
        public int MediaServerGeneration { get; set; }
        public Guid UserId { get; set; }
        public bool Setup { get; set; }
        public bool RegisteredToCurrentAccount { get; set; }

        public string LocalPingUrl { get; set; }
        public string LocalStreamBaseUrl { get; set; }
        public string RemotePingUrl { get; set; }
        public string RemoteStreamBaseUrl { get; set; }
        public bool UseLocalStream { get; set; }
        public string StreamBaseUrl
        {
            get
            {
                return (UseLocalStream ? LocalStreamBaseUrl : RemoteStreamBaseUrl);
            }
        }
    }
}
