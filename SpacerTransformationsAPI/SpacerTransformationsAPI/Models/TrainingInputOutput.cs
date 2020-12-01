using System.Collections.Generic;
using Newtonsoft.Json;

namespace SpacerTransformationsAPI.Models
{
    public class TrainingInputOutput
    {
        [JsonProperty(PropertyName = "input")] public string Input { get; set; }

        [JsonProperty(PropertyName = "output")]
        public string Output { get; set; }

        [JsonProperty(PropertyName = "aux")] 
        public List<string> Commands { get; set; }
    }
}