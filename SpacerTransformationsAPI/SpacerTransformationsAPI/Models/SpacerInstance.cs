using System.Collections.Generic;

namespace SpacerTransformationsAPI.Models
{
    public class SpacerInstance
    {
        public SpacerInstance(string id)
        {
            Id = id;
            Lemmas = new Dictionary<int, Lemma>();
        }
        public string Id { get; set; }
        public Dictionary<int, Lemma> Lemmas { get; set; }
    }
}