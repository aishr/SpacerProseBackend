using System.Collections.Generic;
using Microsoft.ProgramSynthesis.AST;

namespace SpacerTransformationsAPI.Models
{
    public class ProseResponse
    {
        public ProseResponse(string finalProgram, Dictionary<int, Lemma> lemmas)
        {
            FinalProgram = finalProgram;
            Lemmas = lemmas;
        }
        public string FinalProgram { get; }
        public Dictionary<int, Lemma> Lemmas { get; }
    }
}