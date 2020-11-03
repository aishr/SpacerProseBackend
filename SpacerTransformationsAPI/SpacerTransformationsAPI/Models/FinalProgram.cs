using Newtonsoft.Json;

namespace SpacerTransformationsAPI.Models
{
    public class FinalProgram
    {
        public FinalProgram(string humanReadableAst, string xmlAst)
        {
            HumanReadableAst = humanReadableAst;
            XmlAst = xmlAst;
        }
        [JsonProperty(PropertyName = "humanReadableAst")]
        public string HumanReadableAst { get;}
        [JsonProperty(PropertyName = "xmlAst")]
        public string XmlAst { get;}
    }
}