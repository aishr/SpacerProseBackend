using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.Learning;
using Microsoft.ProgramSynthesis.Rules;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.Z3;
using SpacerTransformationsAPI.Functions;
using SpacerTransformationsAPI.Models;

namespace SpacerTransformationsAPI.Prose
{
    public class WitnessFunctions : DomainLearningLogic
    {
        public WitnessFunctions(Grammar grammar) : base(grammar) { }

        #region ToImp Witness Functions 
        //Node ToImp(Node inputTree, List<int> leftSide)
        //output := A -> B
        //input := !A or B
        //1)  input := !A or B ---> !A or B
        //2)  input := !A or B ---> [0]
        [WitnessFunction("ToImp", 0)]
        public ExampleSpec WitnessToImpTree(GrammarRule rule, ExampleSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 0");
            var examples = new Dictionary<State, object>();
            foreach (var input in spec.ProvidedInputs)
            {
                var node = (Node) spec.Examples[input];
                if (node == null || node.Type != Z3_decl_kind.Z3_OP_IMPLIES)
                {
                    return null;
                }
                examples[input] = input.Bindings.First().Value;
                
            }
            return new ExampleSpec(examples);
        }
        
        //Node ToImp(Node inputTree, List<int> leftSide)
        //Given inputTree, I and outputTree, O, WitnessLeftSide(I, O) = R
        //where for every r in R, Transform2(I, r) = O
        // [a, b, c] -> [a], [b], [c], [a,b], [a,c], [a,b,c]
        [WitnessFunction("ToImp", 1)]
        public ExampleSpec WitnessToImpLeftSide(GrammarRule rule, ExampleSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 1");
            var examples = new Dictionary<State, object>();
            foreach (var input in spec.ProvidedInputs)
            {
                var before = (Node) input.Bindings.First().Value;
                var after = (Node) spec.Examples[input];
                var children = before.Children;
                var numLeftSide = after.Type == Z3_decl_kind.Z3_OP_AND ? after.Children.Count : after.Children[0].Type == Z3_decl_kind.Z3_OP_AND ? after.Children[0].Children.Count : 1;
                var listOfIndices = Enumerable.Range(0, children.Count);
                var possibleLists = Utils.GetKCombs(listOfIndices, numLeftSide);
                foreach (var sub in possibleLists)
                {
                    var subList = sub.ToList();
                    if (Semantics.ToImp(before, subList).Equals(after))
                    {
                        examples[input] = subList;
                    }
                }
                
            }
            return new ExampleSpec(examples);
        }
        
        //IEnumerable<int> FilterByName(Node inputTree, string name)
        [WitnessFunction("FilterByName", 0)]
        public ExampleSpec WitnessFilterByNameTree(GrammarRule rule, ExampleSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 0");
            var examples = new Dictionary<State, object>();
            foreach (var input in spec.ProvidedInputs)
            {
                var leftSide = (List<int>) spec.Examples[input];
                if (leftSide == null)
                {
                    return null;
                }
                examples[input] = input.Bindings.First().Value;
            }

            return new ExampleSpec(examples);
        }
        
        //IEnumerable<int> FilterByName(Node inputTree, string name)
        [WitnessFunction("FilterByName", 1)]
        public DisjunctiveExamplesSpec WitnessFilterByNameName(GrammarRule rule, ExampleSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 1");
            var examples = new Dictionary<State, IEnumerable<object>>();
            foreach (var input in spec.ProvidedInputs)
            {
                var before = (Node) input.Bindings.First().Value;
                var after = (List<int>) spec.Examples[input];
                var children = before.GetIdentifiers();
                foreach (var child in children)
                {
                    if (!Semantics.FilterByName(before, child).OrderBy(i => i)
                        .SequenceEqual(after.OrderBy(i => i))) continue;
                    if (!examples.ContainsKey(input))
                    {
                        examples[input] = new List<string>();
                    }
                    ((List<string>) examples[input]).Add(child);
                }
            }

            return new DisjunctiveExamplesSpec(examples);
        }
        
        //IEnumerable<int> FilterByName(Node inputTree, string name)
        [WitnessFunction("FilterByProcess", 0)]
        public ExampleSpec WitnessFilterByProcessTree(GrammarRule rule, ExampleSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 0");
            var examples = new Dictionary<State, object>();
            foreach (var input in spec.ProvidedInputs)
            {
                var leftSide = (List<int>) spec.Examples[input];
                if (leftSide == null)
                {
                    return null;
                }
                examples[input] = input.Bindings.First().Value;
            }

            return new ExampleSpec(examples);
        }
        
        //IEnumerable<int> FilterByProcess(Node inputTree, string process)
        [WitnessFunction("FilterByProcess", 1)]
        public DisjunctiveExamplesSpec WitnessFilterByProcessProcess(GrammarRule rule, ExampleSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 1");
            var examples = new Dictionary<State, IEnumerable<object>>();
            foreach (var input in spec.ProvidedInputs)
            {
                var before = (Node)input.Bindings.First().Value;
                var after = (List<int>)spec.Examples[input];
                var children = before.GetProcesses();    
                foreach (var child in children)
                {
                    if (!Semantics.FilterByProcess(before, child).OrderBy(i => i)
                        .SequenceEqual(after.OrderBy(i => i))) continue;
                    if (!examples.ContainsKey(input))
                    {
                        examples[input] = new List<string>();
                    }
                    ((List<string>)examples[input]).Add(child);
                }
            }

            return new DisjunctiveExamplesSpec(examples);
        }
        
        //IEnumerable<int> FilterByName(Node inputTree, string name)
        [WitnessFunction("FilterStatic", 0)]
        public ExampleSpec WitnessFilterStaticTree(GrammarRule rule, ExampleSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 0");
            var examples = new Dictionary<State, object>();
            foreach (var input in spec.ProvidedInputs)
            {
                var leftSide = (List<int>) spec.Examples[input];
                if (leftSide == null)
                {
                    return null;
                }
                examples[input] = input.Bindings.First().Value;
            }

            return new ExampleSpec(examples);
        }
        
        //IEnumerable<int> FilterStatic(Node inputTree, StaticFilterType type)
        [WitnessFunction("FilterStatic", 1)]
        public DisjunctiveExamplesSpec WitnessFilterStaticType(GrammarRule rule, ExampleSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 1");
            var examples = new Dictionary<State, IEnumerable<object>>();
            foreach (var input in spec.ProvidedInputs)
            {
                var before = (Node)input.Bindings.First().Value;
                var after = (List<int>)spec.Examples[input];
                foreach (StaticFilterType type in Enum.GetValues(typeof(StaticFilterType)))
                {
                    if (!Semantics.FilterStatic(before, type).OrderBy(i => i)
                        .SequenceEqual(after.OrderBy(i => i))) continue;
                    if (!examples.ContainsKey(input))
                    {
                        examples[input] = new List<object>();
                    }
                    ((List<object>)examples[input]).Add(type);
                }
            }

            return new DisjunctiveExamplesSpec(examples);
            
        }
        #endregion
        
        #region Move Witness Functions
        //Node Move(Node inputTree, int position, Tuple<int, bool> direction)
        [WitnessFunction("Move", 0)]
        public DisjunctiveExamplesSpec WitnessMoveTree(GrammarRule rule, ExampleSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 0");
            var examples = new Dictionary<State, IEnumerable<object>>();
            foreach (var input in spec.ProvidedInputs)
            {
                var node = (Node) spec.Examples[input];
                if (node == null)
                {
                    return null;
                }

                if (!examples.ContainsKey(input))
                {
                    examples[input] = new List<object>();
                }

                var children = node.Children;
                for (var i = 0; i < children.Count; i++)
                {
                    var possiblePlacesTrue = GeneratePossiblePlaces(children.Count, i, true);
                    var possiblePlacesFalse = GeneratePossiblePlaces(children.Count, i, false);

                    foreach (var places in possiblePlacesTrue)
                    {
                        var direction = new Tuple<int, bool>(places, true);
                        var actual = Semantics.Move(node, i, direction);
                        if (actual != null && !examples[input].Contains(actual))
                        {
                            ((List<object>) examples[input]).Add(actual);
                        }
                    }
                    foreach (var places in possiblePlacesFalse)
                    {
                        var direction = new Tuple<int, bool>(places, false);
                        var actual = Semantics.Move(node, i, direction);
                        if (actual != null && !examples[input].Contains(actual))
                        {
                            ((List<object>) examples[input]).Add(actual);
                        }
                    }
                }

            }

            return new DisjunctiveExamplesSpec(examples);
        }
        
        //Node Move(Node inputTree, int position, Tuple<int, bool> direction)
        [WitnessFunction("Move", 1, DependsOnParameters = new []{0})]

        public DisjunctiveExamplesSpec WitnessMovePosition(GrammarRule rule, ExampleSpec spec, DisjunctiveExamplesSpec treeSpec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 1");
            var examples = new Dictionary<State, IEnumerable<object>>();
            foreach (var input in spec.ProvidedInputs)
            {
                var before = (Node)input.Bindings.First().Value;
                var after = (Node)spec.Examples[input];
                var possiblePositions = WitnessMovePositionHelper(before, after);
                for (var i = 0; i < after.Children.Count; ++i)
                {
                    if (!possiblePositions.Contains(i)) continue;
                    if (!examples.ContainsKey(input))
                    {
                        examples[input] = new List<object>();
                    }
                    ((List<object>)examples[input]).Add(i);
                }
            }

            return new DisjunctiveExamplesSpec(examples);
        }

        private static IEnumerable<int> WitnessMovePositionHelper(Node inputTree, Node outputTree)
        {
            var result = new List<int>();
            if (outputTree.Type == Z3_decl_kind.Z3_OP_IMPLIES) return result;
            var inputChildren = inputTree.Children;
            var outputChildren = outputTree.Children;

            for (var i = 0; i < inputChildren.Count; i++)
            {
                if (!inputChildren[i].Equals(outputChildren[i]))
                {
                    result.Add(i);
                }
            }

            return result;
        }

        //Node Move(Node inputTree, int position, Tuple<int, bool> direction)
        [WitnessFunction("Move", 2, DependsOnParameters = new[] {1})]
        public DisjunctiveExamplesSpec WitnessMoveLeft(GrammarRule rule, ExampleSpec spec,
            ExampleSpec positionSpec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 2");
            var examples = new Dictionary<State, IEnumerable<object>>();
            foreach (var input in spec.Examples)
            {
                var inputState = input.Key;
                var before = (Node) inputState.Bindings.First().Value;
                var position = (int)positionSpec.Examples[inputState];
                var after = (Node) spec.Examples[inputState];

                var possibleLeft = new List<bool>() {true, false};
                foreach (var left in possibleLeft)
                {
                    var possiblePlaces = GeneratePossiblePlaces(before.Children.Count, position, left);
                    foreach (var places in possiblePlaces)
                    {
                        var direction = new Tuple<int, bool>(places, left);
                        var actual = Semantics.Move(before, position, direction);
                        if (actual == null || !actual.Equals(after)) continue;
                        if (!examples.ContainsKey(inputState))
                        {
                            examples[inputState] = new List<object>();
                        }

                        ((List<object>) examples[inputState]).Add(direction);
                    }
                }
            }

            return new DisjunctiveExamplesSpec(examples);
        }

        private static IEnumerable<int> GeneratePossiblePlaces(int numOfChildren, int position, bool left)
        {
            var result = new List<int>();
            switch (left)
            {
                case true when position > 0:
                {
                    var possiblePlaces = 1;
                    var difference = position - possiblePlaces;
                    while (difference >= 0)
                    {
                        result.Add(possiblePlaces);
                        possiblePlaces++;
                        difference = position - possiblePlaces;
                    }

                    break;
                }
                case false when position < numOfChildren - 1:
                {
                    var possiblePlaces = 1;
                    var difference = position + possiblePlaces;
                    while (difference < numOfChildren)
                    {
                        result.Add(possiblePlaces);
                        possiblePlaces++;
                        difference = position + possiblePlaces;
                    }

                    break;
                }
            }

            return result;
        }

        //int IndexByName(Node inputTree, string name)
        [WitnessFunction("IndexByName", 0)]
        public ExampleSpec WitnessIndexByNameTree(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 0");
            var examples = new Dictionary<State, object>();
            foreach (var input in spec.ProvidedInputs)
            {
                var indexes = spec.DisjunctiveExamples[input];
                
                if (!indexes.All(x => (int)x >= 0))
                {
                    return null;
                }
                examples[input] = input.Bindings.First().Value;
            }

            return new ExampleSpec(examples);
        }

        //int IndexByName(Node inputTree, string name)
        [WitnessFunction("IndexByName", 1)]
        public DisjunctiveExamplesSpec WitnessIndexByNameName(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 1");
            var examples = new Dictionary<State, IEnumerable<object>>();
            foreach (var input in spec.ProvidedInputs)
            {
                var before = (Node) input.Bindings.First().Value;
                var possibleIndexes = spec.DisjunctiveExamples[input];
                var children = before.GetIdentifiers();

                foreach (var child in children)
                {
                    if (!possibleIndexes.Contains(Semantics.IndexByName(before, child))) continue;
                    if (!examples.ContainsKey(input))
                    {
                        examples[input] = new List<object>();
                    }
                    ((List<object>)examples[input]).Add(child);
                }
            }

            return new DisjunctiveExamplesSpec(examples);
        }

        //int IndexFromFront(Node inputTree, int index)
        [WitnessFunction("IndexFromFront", 0)]
        public ExampleSpec WitnessIndexFromFrontTree(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 0");
            var examples = new Dictionary<State, object>();
            foreach (var input in spec.ProvidedInputs)
            {
                var indexes = spec.DisjunctiveExamples[input];
                
                if (!indexes.All(x => (int)x >= 0))
                {
                    return null;
                }
                examples[input] = input.Bindings.First().Value;
            }

            return new ExampleSpec(examples);
        }
        
        //int IndexFromFront(Node inputTree, int index)
        [WitnessFunction("IndexFromFront", 1)]
        public DisjunctiveExamplesSpec WitnessIndexFromFrontIndex(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 1");
            var examples = new Dictionary<State, IEnumerable<object>>();
            foreach (var input in spec.ProvidedInputs)
            {
                var before = (Node) input.Bindings.First().Value;
                var possibleIndexes = spec.DisjunctiveExamples[input];

                for (var i = 0; i < before.Children.Count; ++i)
                {
                    if (!possibleIndexes.Contains(Semantics.IndexFromFront(before, i))) continue;
                    if (!examples.ContainsKey(input))
                    {
                        examples[input] = new List<object>();
                    }
                    ((List<object>)examples[input]).Add(i);
                }
            }

            return new DisjunctiveExamplesSpec(examples);
        }

        //int IndexFromBack(Node inputTree, int index)
        [WitnessFunction("IndexFromBack", 0)]
        public ExampleSpec WitnessIndexFromBackTree(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 0");
            var examples = new Dictionary<State, object>();
            foreach (var input in spec.ProvidedInputs)
            {
                var indexes = spec.DisjunctiveExamples[input];
                
                if (!indexes.All(x => (int)x >= 0))
                {
                    return null;
                }
                examples[input] = input.Bindings.First().Value;
            }

            return new ExampleSpec(examples);
        }
        
        //int IndexFromBack(Node inputTree, int index)
        [WitnessFunction("IndexFromBack", 1)]
        public DisjunctiveExamplesSpec WitnessIndexFromBackIndex(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 1");
            var examples = new Dictionary<State, IEnumerable<object>>();
            foreach (var input in spec.ProvidedInputs)
            {
                var before = (Node) input.Bindings.First().Value;
                var possibleIndexes = spec.DisjunctiveExamples[input];

                for (var i = 0; i < before.Children.Count; ++i)
                {
                    if (!possibleIndexes.Contains(Semantics.IndexFromBack(before, i))) continue;
                    if (!examples.ContainsKey(input))
                    {
                        examples[input] = new List<object>();
                    }
                    ((List<object>)examples[input]).Add(i);
                }
            }

            return new DisjunctiveExamplesSpec(examples);
        }
        #endregion

        #region SquashNegation Witness Functions
        /*
        //Node SquashNegation(Node inputTree, string symbol)
        [WitnessFunction("SquashNegation", 0)]
        public ExampleSpec WitnessSquashNegationTree(GrammarRule rule, ExampleSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 0");
            var examples = new Dictionary<State, object>();
            foreach (var input in spec.ProvidedInputs)
            {
                var node = (Node) spec.Examples[input];
                if (node == null || node.Type != Z3_decl_kind.Z3_OP_NOT)
                {
                    return null;
                }
                examples[input] = input.Bindings.First().Value;
            }

            return new ExampleSpec(examples);
        }
        
        //Node SquashNegation(Node inputTree, string symbol)
        [WitnessFunction("SquashNegation", 1)]
        public ExampleSpec WitnessSquashNegationSymbol(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 1");
            var examples = new Dictionary<State, object>();
            foreach (var input in spec.ProvidedInputs)
            {
                 var before = (Node) input.Bindings.First().Value;
                var after = (Node)spec.Examples[input];

                var negatable = new List<string>()
                    {
                        "not",
                        "<",
                        ">",
                        "<=",
                        ">="
                    };

                foreach (var symbol in negatable)
                {
                    if (Semantics.SquashNegation(before, symbol).Equals(after))
                    {
                        examples[input] = symbol;
                    }
                }
            }

            return new ExampleSpec(examples);
        }
        */
        #endregion

        #region FlipComparison Witness Functions
        //Node FlipComparison(Node inputTree, string symbol, bool flip)
        [WitnessFunction("FlipComparison", 0)]
        public ExampleSpec WitnessFlipComparisonTree(GrammarRule rule, ExampleSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 0");
            var examples = new Dictionary<State, object>();
            foreach (var input in spec.ProvidedInputs)
            {
                var flippable = new List<Z3_decl_kind>
                {
                    Z3_decl_kind.Z3_OP_LT,
                    Z3_decl_kind.Z3_OP_GT,
                    Z3_decl_kind.Z3_OP_LE,
                    Z3_decl_kind.Z3_OP_GE
                };
                var node = (Node) spec.Examples[input];
                if (node == null || !flippable.Contains(node.Type))
                {
                    return null;
                }

                examples[input] = input.Bindings.First().Value;
            }

            return new ExampleSpec(examples);
        }
       
        //Node FlipComparison(Node inputTree, string symbol, bool flip)
        [WitnessFunction("FlipComparison", 1)]
        public ExampleSpec WitnessFlipComparisonSymbol(GrammarRule rule, ExampleSpec spec)
        {
            var examples = new Dictionary<State, object>();
            foreach (var input in spec.ProvidedInputs)
            {
                var before = (Node) input.Bindings.First().Value;
                var after = (Node) spec.Examples[input];

                var flippable = new List<string>()
                {
                    "<",
                    ">",
                    "<=",
                    ">="
                };

                foreach (var symbol in flippable)
                {
                    var actual = Semantics.FlipComparison(before, symbol, true);
                    if (actual != null && actual.Equals(after))
                    {
                        examples[input] = symbol;
                    }
                }
            }

            return new ExampleSpec(examples);
        }

        //Node FlipComparison(Node inputTree, string symbol, bool flip)
        [WitnessFunction("FlipComparison", 2)]
        public ExampleSpec WitnessFlipComparisonFlip(GrammarRule rule, ExampleSpec spec)
        {
            var examples = new Dictionary<State, object>();
            foreach (var input in spec.ProvidedInputs)
            {
                examples[input] = true;
            }

            return new ExampleSpec(examples);
        } 

        //bool FlipByName(Node inputTree, string name)
        [WitnessFunction("FlipByName", 0)]
        public ExampleSpec WitnessFlipByNameTree(GrammarRule rule, ExampleSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 0");
            var examples = new Dictionary<State, object>();
            foreach (var input in spec.ProvidedInputs)
            {
                examples[input] = input.Bindings.First().Value;
            }

            return new ExampleSpec(examples);
        }
        
        //bool FlipByName(Node inputTree, string name)
        [WitnessFunction("FlipByName", 1)]
        public DisjunctiveExamplesSpec WitnessFlipByNameName(GrammarRule rule, ExampleSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 1");
            var examples = new Dictionary<State, IEnumerable<object>>();
            foreach (var input in spec.ProvidedInputs)
            {
                var before = (Node) input.Bindings.First().Value;
                var after = (bool)spec.Examples[input];

                var flippable = new List<string>()
                    {
                        "<",
                        ">",
                        "<=",
                        ">="
                    };

                var children = before.GetIdentifiers();

                foreach (var child in from symbol in flippable from child in children where Semantics.FlipByName(before, child) == after select child)
                {
                    if (!examples.ContainsKey(input))
                    {
                        examples[input] = new List<object>();
                    }

                    ((List<object>)examples[input]).Add(child);
                }
            }

            return new DisjunctiveExamplesSpec(examples);
        }
        
        //bool FlipByProcess(Node inputTree, string process)
        [WitnessFunction("FlipByProcess", 0)]
        public ExampleSpec WitnessFlipByProcessTree(GrammarRule rule, ExampleSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 0");
            var examples = new Dictionary<State, object>();
            foreach (var input in spec.ProvidedInputs)
            {
                examples[input] = input.Bindings.First().Value;
            }

            return new ExampleSpec(examples);
        }

        //bool FlipByProcess(Node inputTree, string process)
        [WitnessFunction("FlipByProcess", 1)]
        public DisjunctiveExamplesSpec WitnessFlipByProcessProcess(GrammarRule rule, ExampleSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 1");
            var examples = new Dictionary<State, IEnumerable<object>>();
            foreach (var input in spec.ProvidedInputs)
            {
                var before = (Node) input.Bindings.First().Value;
                var after = (bool)spec.Examples[input];

                var flippable = new List<string>()
                    {
                        "<",
                        ">",
                        "<=",
                        ">="
                    };

                var children = before.GetProcesses();

                foreach (var child in from symbol in flippable from child in children where Semantics.FlipByProcess(before, child) == after select child)
                {
                    if (!examples.ContainsKey(input))
                    {
                        examples[input] = new List<object>();
                    }

                    ((List<object>)examples[input]).Add(child);
                }
            }

            return new DisjunctiveExamplesSpec(examples);
        }
        #endregion
    }
}