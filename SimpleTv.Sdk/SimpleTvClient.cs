using System.Collections.Generic;

using NodaTime;

using SimpleTv.Sdk.Models;
using SimpleTv.Sdk.Http;

namespace SimpleTv.Sdk
{
    public class SimpleTvClient
    {
        SimpleTvHttpClient _client;

        public SimpleTvClient()
        {
            _client = new SimpleTvHttpClient(SystemClock.Instance, DateTimeZoneProviders.Bcl);
        }

        public bool Login(string username, string password)
        {
            return _client.Login(username, password);
        }


        private List<MediaServer> _mediaServers;
        public List<MediaServer> MediaServers
        {
            get
            {
                if (_mediaServers == null)
                {
                    _mediaServers = _client.GetMediaServers();
                }
                return _mediaServers;
            }
        }
    }
}
