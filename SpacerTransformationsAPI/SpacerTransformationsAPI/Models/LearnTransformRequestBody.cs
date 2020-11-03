using System.Collections.Generic;
using Newtonsoft.Json;

namespace SpacerTransformationsAPI.Models
{
    public class LearnTransformRequestBody
    {
        [JsonProperty(PropertyName = "instance")]
        public string Instance { get; set; }
        [JsonProperty(PropertyName = "declareStatements")]
        public List<string> DeclareStatements { get; set; }
    }
}