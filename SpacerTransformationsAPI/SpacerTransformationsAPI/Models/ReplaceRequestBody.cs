using System.Collections.Generic;
using Newtonsoft.Json;

namespace SpacerTransformationsAPI.Models
{
    public class ReplaceRequestBody
    {
        
        [JsonProperty(PropertyName = "instance")]
        public string Instance { get; set; }
        [JsonProperty(PropertyName = "inputOutputExamples")]
        public List<TrainingInputOutput> InputOutputExamples { get; set; }
        [JsonProperty(PropertyName = "params")]
        public List<Params> Params { get; set; }
        [JsonProperty(PropertyName = "exprMap")]
        public string ExprMap { get; set; }
    }
}
