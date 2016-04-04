using System;

using SimpleTv.Sdk.Http;
using SimpleTv.Sdk.Naming;

namespace SimpleTv.Sdk.Models
{
    public class Episode
    {
        private SimpleTvHttpClient _client;
        internal Show show;
        public Episode(SimpleTvHttpClient client, Show show)
        {
            this._client = client;
            this.show = show;
        }

        public void Download(string baseFolder, string folderFormat, string fileNameTemplate)
        {
            var fileName = this.GenerateFileName(baseFolder, folderFormat, fileNameTemplate);
            _client.Download(this, fileName);
        }

        public Guid Id { get; set; }
        public Guid InstanceId { get; set; }
        public string EpisodeName { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public string Description { get; set; }

        public string ChannelNumber { get; set; }
        public DateTime? DateTime { get; set; }
    }
}
