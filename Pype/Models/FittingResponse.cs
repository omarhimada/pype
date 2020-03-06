using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Pype.Models
{
  public class FittingResponse
  {
    [JsonProperty("status")]
    public FittingResponseStatus Status { get; set; }

    [JsonProperty("result")]
    public JObject Result { get; set; }
  }
}
