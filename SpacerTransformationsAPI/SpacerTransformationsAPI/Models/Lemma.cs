using System.Collections.Generic;

namespace SpacerTransformationsAPI.Models
{
    public class Lemma
    {
        public string Edited { get; set; }
        public string Raw { get; set; }
        public string Readable { get; set; }
        public List<int> Lhs { get; set; }
        public bool Changed { get; set; }
        
    }
}