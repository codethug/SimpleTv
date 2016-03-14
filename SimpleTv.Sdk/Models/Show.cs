using System;
using System.Collections.Generic;

using SimpleTv.Sdk.Http;

namespace SimpleTv.Sdk.Models
{
    public class Show
    {
        private SimpleTvHttpClient _client;
        internal MediaServer server;

        public Show(SimpleTvHttpClient client, MediaServer server)
        {
            this._client = client;
            this.server = server;
        }

        private List<Episode> _episodes;
        public List<Episode> Episodes
        {
            get
            {
                if (_episodes == null)
                {
                    _episodes = _client.GetEpisodes(this);
                }
                return _episodes;
            }
        }

        public Guid Id { get; set; } // Group Id
        public string Name { get; set; }
        public int NumEpisodes { get; set; }

    }
}
