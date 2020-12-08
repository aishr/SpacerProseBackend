using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.AST;
using Microsoft.ProgramSynthesis.Features;

namespace SpacerTransformationsAPI.Prose
{
    public class RankingScore : Feature<double>
    {
        public RankingScore(Grammar grammar) : base(grammar, "Score", isComplete: true) { }
        protected override double GetFeatureValueForVariable(VariableNode variable) => 0;
        public static double ScoreForContext = 0;
        
        [FeatureCalculator("Transform")]
        public static double Score_Transform(double inputTree, double leftSide) => leftSide;

        [FeatureCalculator("Filter")]
        public static double Score_Filter(double inputTree, double name) => name;
        
        [FeatureCalculator("FilterAllButLast")]
        public static double Score_FilterAllButLast(double inputTree) => 2;
        
        [FeatureCalculator("FilterByProcess")]
        public static double Score_FilterByProcess(double inputTree, double process) => process;
        
        [FeatureCalculator("FilterByNot")]
        public static double Score_FilterByNot(double inputTree) => 2;
        
        [FeatureCalculator("name", Method = CalculationMethod.FromLiteral)]
        public static double NameScore(string type) => (type.Equals("any")) ? 1 : 2;
        
        [FeatureCalculator("process", Method = CalculationMethod.FromLiteral)]
        public static double ProcessScore(string type) => (type.Equals("any")) ? 1 : 2;
    }
}