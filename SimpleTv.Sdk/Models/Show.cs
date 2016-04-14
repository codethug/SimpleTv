using System;
using System.Collections.Generic;

using SimpleTv.Sdk.Http;

namespace SimpleTv.Sdk.Models
{
    public class Show
    {
        public Guid Id { get; set; } // Group Id
        public string Name { get; set; }
        public int NumEpisodes { get; set; }

        public MediaServer Server { get; set; }
    }
}
