using Newtonsoft.Json;

namespace SpacerTransformationsAPI.Models
{
    public class ApplyTransformRequestBody
    {
        [JsonProperty(PropertyName = "instance")]
        public string Instance { get; set; }
        [JsonProperty(PropertyName = "program")]
        public string Program { get; set; }
        [JsonProperty(PropertyName = "declareStatements")]
        public string DeclareStatements { get; set; }
        [JsonProperty(PropertyName = "exprMap")]
        public string ExprMap { get; set; }
    }
}
