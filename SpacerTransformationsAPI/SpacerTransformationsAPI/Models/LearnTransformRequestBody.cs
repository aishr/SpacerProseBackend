using System.Collections.Generic;
using Newtonsoft.Json;

namespace SpacerTransformationsAPI.Models
{
    public class LearnTransformRequestBody
    {
        
        [JsonProperty(PropertyName = "instance")]
        public string Instance { get; set; }
        [JsonProperty(PropertyName = "declareStatements")]
        public string DeclareStatements { get; set; }
        [JsonProperty(PropertyName = "inputOutputExamples")]
        public List<TrainingInputOutput> InputOutputExamples { get; set; }
        [JsonProperty(PropertyName = "spacerInstance")]
        public string SpacerInstance { get; set; }
    }
}
