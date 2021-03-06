﻿using System;

using SimpleTv.Sdk.Http;
using SimpleTv.Sdk.Naming;

namespace SimpleTv.Sdk.Models
{
    public class Episode
    {
        public Guid Id { get; set; }
        public Guid InstanceId { get; set; }
        public string EpisodeName { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public string Description { get; set; }

        public string ChannelNumber { get; set; }
        public DateTime? DateTime { get; set; }
        public string Error { get; set; }


        public Show Show { get; set; }

    }
}
