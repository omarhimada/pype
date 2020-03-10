using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Pype.Models
{
    public class FittingResponse
    {
        [JsonProperty("status")]
        public FittingResponseStatus Status { get; set; }

        [JsonProperty("result")]
        public JToken Result { get; set; }

        [JsonProperty("exceptionThrown")]
        public Exception ExceptionThrown { get; set; }
    }
}
