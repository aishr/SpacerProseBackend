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
        public static double Score_ToImp(double inputTree, double leftSide) => leftSide;

        [FeatureCalculator("FilterByName")]
        public static double Score_Filter(double inputTree, double name) => name;
        
        [FeatureCalculator("FilterStatic")]
        public static double Score_FilterStatic(double inputTree, double type) => type;
        
        [FeatureCalculator("FilterByProcess")]
        public static double Score_FilterByProcess(double inputTree, double process) => process;
        
        [FeatureCalculator("Move")]
        public static double Score_Move(double inputTree, double position, double direction) => position;

        [FeatureCalculator("IndexByName")]
        public static double Score_IndexByName(double inputTree, double name) => name;

        [FeatureCalculator("IndexFromFront")]
        public static double Score_IndexFromFront(double inputTree, double index) => index;

        [FeatureCalculator("IndexFromBack")]
        public static double Score_IndexFromBack(double inputTree, double index) => index;
        
        [FeatureCalculator("SquashNegation")]
        public static double Score_SquashNegation(double inputTree, double symbol) => symbol;

        [FeatureCalculator("FlipComparison")]
        public static double Score_FlipComparison(double inputTree, double symbol, double flip) => symbol;

        [FeatureCalculator("FlipByName")]
        public static double Score_FlipByName(double inputTree, double name) => name;

        [FeatureCalculator("FlipByProcess")]
        public static double Score_FlipByProcess(double inputTree, double process) => process;
        
        [FeatureCalculator("name", Method = CalculationMethod.FromLiteral)]
        public static double NameScore(string type) => (type.Equals("any")) ? 1 : 3;
        
        [FeatureCalculator("process", Method = CalculationMethod.FromLiteral)]
        public static double ProcessScore(string type) => (type.Equals("any")) ? 1 : 3;
        
        [FeatureCalculator("type", Method = CalculationMethod.FromLiteral)]
        public static double TypeScore(StaticFilterType type) => 3;

        [FeatureCalculator("index", Method = CalculationMethod.FromLiteral)]
        public static double IndexScore(int type) => type < 0 ? 1 : 3;
        
        [FeatureCalculator("symbol", Method = CalculationMethod.FromLiteral)]
        public static double SymbolScore(string type) => (type.Equals("any")) ? 1 : 3;
        
        [FeatureCalculator("left", Method = CalculationMethod.FromLiteral)]
        public static double LeftScore(bool type) => 3;
    }
}
