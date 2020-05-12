using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Pype.Models
{
    public class FittingResponse<T> where T : class
    {
        [JsonProperty("status")]
        public FittingResponseStatus Status { get; set; }

        [JsonProperty("result")]
        public T Result { get; set; }

        [JsonProperty("exceptionThrown")]
        public Exception ExceptionThrown { get; set; }
    }
}
