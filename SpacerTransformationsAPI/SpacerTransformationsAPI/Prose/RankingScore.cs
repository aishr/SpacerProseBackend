using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.AST;
using Microsoft.ProgramSynthesis.Features;
using SpacerTransformationsAPI.Models;

namespace SpacerTransformationsAPI.Prose
{
    public class RankingScore : Feature<double>
    {
        public RankingScore(Grammar grammar) : base(grammar, "Score", isComplete: true) { }
        protected override double GetFeatureValueForVariable(VariableNode variable) => 0;
        public static double ScoreForContext = 0;
        
        [FeatureCalculator("ToImp")]
        public static double Score_ToImp(double inputTree, double leftSide) => inputTree + leftSide;

        [FeatureCalculator("JoinFilters")]
        public static double Score_JoinFilters(double filter1, double filter2) => filter1 + filter2;

        [FeatureCalculator("FilterByName")]
        public static double Score_Filter(double inputTree, double name) => inputTree + name;
        
        [FeatureCalculator("FilterStatic")]
        public static double Score_FilterStatic(double inputTree, double type) => inputTree + type;
        
        [FeatureCalculator("FilterByArrayIndex")]
        public static double Score_FilterByArrayIndex(double inputTree, double process) => inputTree + process;
        
        [FeatureCalculator("name", Method = CalculationMethod.FromLiteral)]
        public static double NameScore(string type) => (type.Equals("any")) ? 0 : 1;
        
        [FeatureCalculator("index", Method = CalculationMethod.FromLiteral)]
        public static double IndexScore(string type) => (type.Equals("any")) ? 0 : 1;
        
        [FeatureCalculator("type", Method = CalculationMethod.FromLiteral)]
        public static double TypeScore(StaticFilterType type) => 2;

    }
}
