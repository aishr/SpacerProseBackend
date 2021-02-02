using Newtonsoft.Json;

namespace SpacerTransformationsAPI.Models
{
    public class Lemma
    {
        [JsonProperty(PropertyName = "raw")]
        public string Raw { get; set; }
        [JsonProperty(PropertyName = "readable")]
        public string Readable { get; set; }
    }
}
