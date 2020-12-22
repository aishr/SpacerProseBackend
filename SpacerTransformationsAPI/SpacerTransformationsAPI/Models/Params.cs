using Newtonsoft.Json;

namespace SpacerTransformationsAPI.Models
{
    public class Params
    {
        [JsonProperty(PropertyName = "regex")]
        public bool Regex { get; set; }
        [JsonProperty(PropertyName = "source")]
        public string Source { get; set; }
        [JsonProperty(PropertyName = "target")]
        public string Target { get; set; }
    }
}