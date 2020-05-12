using Newtonsoft.Json;
using System;

namespace Pype.Models
{
    public class FittingResponseStatus
    {
        [JsonProperty("health")]
        public string Health { get; set; }

        [JsonProperty("requestUtcDateTime")]
        public DateTime RequestUtcDateTime { get; set; }

        [JsonProperty("responseUtcDateTime")]
        public DateTime ResponseUtcDateTime { get; set; }
    }
}
