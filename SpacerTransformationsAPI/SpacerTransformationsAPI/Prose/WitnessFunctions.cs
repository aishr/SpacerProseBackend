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
                    if (Semantics.FilterByName(before, child).OrderBy(i => i)
                        .SequenceEqual(after.OrderBy(i => i)))
                    {
                        if (!examples.ContainsKey(input))
                        {
                            examples[input] = new List<string>();
                        }
                        ((List<string>) examples[input]).Add(child);
                    }
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
                    if (Semantics.FilterByProcess(before, child).OrderBy(i => i)
                        .SequenceEqual(after.OrderBy(i => i)))
                    {
                        if (!examples.ContainsKey(input))
                        {
                            examples[input] = new List<string>();
                        }
                        ((List<string>)examples[input]).Add(child);
                    }
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
                    if (Semantics.FilterStatic(before, type).OrderBy(i => i)
                        .SequenceEqual(after.OrderBy(i => i)))
                    {
                        if (!examples.ContainsKey(input))
                        {
                            examples[input] = new List<object>();
                        }
                        ((List<object>)examples[input]).Add(type);
                    }
                }
            }

            return new DisjunctiveExamplesSpec(examples);
            
        }
        
        //Node Move(Node inputTree, int position, bool left)
        [WitnessFunction("Move", 0)]
        public ExampleSpec WitnessMoveTree(GrammarRule rule, ExampleSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 0");
            var examples = new Dictionary<State, object>();
            foreach (var input in spec.ProvidedInputs)
            {
                var node = (Node) spec.Examples[input];
                if (node == null)
                {
                    return null;
                }
                examples[input] = input.Bindings.First().Value;
            }

            return new ExampleSpec(examples);
        }
        
        //Node Move(Node inputTree, int position, bool left)
        [WitnessFunction("Move", 1)]
        public ExampleSpec WitnessMovePosition(GrammarRule rule, ExampleSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 1");
            var examples = new Dictionary<State, object>();
            foreach (var input in spec.ProvidedInputs)
            {
                var before = (Node)input.Bindings.First().Value;
                var after = (Node)spec.Examples[input];
                for (var i = 0; i < after.Children.Count; ++i)
                {
                    var actual = Semantics.Move(before, i, true);
                    if (actual != null && actual.Equals(after))
                    {
                        examples[input] = i;
                    }

                    actual = Semantics.Move(before, i, false);
                    if (actual != null && actual.Equals(after))
                    {
                        examples[input] = i;
                    }
                }
            }

            return new ExampleSpec(examples);
        }
        
        //Node Move(Node inputTree, int position, bool left)
        [WitnessFunction("Move", 2, DependsOnParameters = new[] {1})]
        public DisjunctiveExamplesSpec WitnessMoveLeft(GrammarRule rule, ExampleSpec spec, ExampleSpec positionSpec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 2");
            var examples = new Dictionary<State, IEnumerable<object>>();
            foreach (var input in spec.Examples)
            {
                var inputState = input.Key; 
                var before = (Node) inputState.Bindings.First().Value;
                var position = (int)positionSpec.Examples[inputState];
                var after = (Node)spec.Examples[inputState];
                var actual = Semantics.Move(before, position, true);
                if (actual != null && actual.Equals(after))
                {
                    if (!examples.ContainsKey(inputState))
                    {
                        examples[inputState] = new List<object>();
                    }

                    if (!(examples[inputState]).Contains(true))
                    {
                        ((List<object>)examples[inputState]).Add(true);
                    }
                }

                actual = Semantics.Move(before, position, false);
                if (actual != null && actual.Equals(after))
                {
                    if (!examples.ContainsKey(inputState))
                    {
                        examples[inputState] = new List<object>();
                    }

                    if (!(examples[inputState]).Contains(false))
                    {
                        ((List<object>)examples[inputState]).Add(false);
                    }
                }
            }

            return new DisjunctiveExamplesSpec(examples);
        }
        
        //int IndexByName(Node inputTree, string name)
        [WitnessFunction("IndexByName", 0)]
        public ExampleSpec WitnessIndexByNameTree(GrammarRule rule, ExampleSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 0");
            var examples = new Dictionary<State, object>();
            foreach (var input in spec.ProvidedInputs)
            {
                var index = (int) spec.Examples[input];
                if (index < 0)
                {
                    return null;
                }
                examples[input] = input.Bindings.First().Value;
            }

            return new ExampleSpec(examples);
        }

        //int IndexByName(Node inputTree, string name)
        [WitnessFunction("IndexByName", 1)]
        public ExampleSpec WitnessIndexByNameName(GrammarRule rule, ExampleSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 1");
            var examples = new Dictionary<State, object>();
            foreach (var input in spec.ProvidedInputs)
            {
                var before = (Node) input.Bindings.First().Value;
                var after = (int)spec.Examples[input];
                var children = before.GetIdentifiers();

                foreach (var child in children)
                {
                    if (Semantics.IndexByName(before, child) == after)
                    {
                        examples[input] = child;
                    }
                }
            }

            return new ExampleSpec(examples);
        }

        //int IndexFromFront(Node inputTree, int index)
        [WitnessFunction("IndexFromFront", 0)]
        public ExampleSpec WitnessIndexFromFrontTree(GrammarRule rule, ExampleSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 0");
            var examples = new Dictionary<State, object>();
            foreach (var input in spec.ProvidedInputs)
            {
                var index = (int) spec.Examples[input];
                if (index < 0)
                {
                    return null;
                }
                examples[input] = input.Bindings.First().Value;
            }

            return new ExampleSpec(examples);
        }
        
        //int IndexFromFront(Node inputTree, int index)
        [WitnessFunction("IndexFromFront", 1)]
        public ExampleSpec WitnessIndexFromFrontIndex(GrammarRule rule, ExampleSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 1");
            var examples = new Dictionary<State, object>();
            foreach (var input in spec.ProvidedInputs)
            {
                var before = (Node) input.Bindings.First().Value;
                var after = (int)spec.Examples[input];

                for (var i = 0; i < before.Children.Count; ++i)
                {
                    if (Semantics.IndexFromFront(before, i) == after)
                    {
                        examples[input] = i;
                    }
                }
            }

            return new ExampleSpec(examples);
        }

        //int IndexFromBack(Node inputTree, int index)
        [WitnessFunction("IndexFromBack", 0)]
        public ExampleSpec WitnessIndexFromBackTree(GrammarRule rule, ExampleSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 0");
            var examples = new Dictionary<State, object>();
            foreach (var input in spec.ProvidedInputs)
            {
                var index = (int) spec.Examples[input];
                if (index < 0)
                {
                    return null;
                }
                examples[input] = input.Bindings.First().Value;
            }

            return new ExampleSpec(examples);
        }
        
        //int IndexFromBack(Node inputTree, int index)
        [WitnessFunction("IndexFromBack", 1)]
        public ExampleSpec WitnessIndexFromBackIndex(GrammarRule rule, ExampleSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 1");
            var examples = new Dictionary<State, object>();
            foreach (var input in spec.ProvidedInputs)
            {
                var before = (Node) input.Bindings.First().Value;
                var after = (int)spec.Examples[input];

                for (var i = 0; i < before.Children.Count; ++i)
                {
                    if (Semantics.IndexFromBack(before, i) == after)
                    {
                        examples[input] = i;
                    }
                }
            }

            return new ExampleSpec(examples);
        }

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
        public ExampleSpec WitnessSquashNegationSymbol(GrammarRule rule, ExampleSpec spec)
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
                    Console.WriteLine("null");
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
                var before = (Node)input[rule.Body[0]];
                var after = (Node)spec.Examples[input];

                var flippable = new List<string>()
                {
                    "<",
                    ">",
                    "<=",
                    ">="
                };

                foreach (var symbol in flippable)
                {
                    if (Semantics.FlipComparison(before, symbol, true).IsEqualTo(after))
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

                foreach (var symbol in flippable)
                {
                    foreach (var child in children)
                    {
                        if (Semantics.FlipByName(before, child) == after)
                        {
                            if (!examples.ContainsKey(input))
                            {
                                examples[input] = new List<object>();
                            }

                            ((List<object>)examples[input]).Add(child);
                        }
                    }
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

                foreach (var symbol in flippable)
                {
                    foreach (var child in children)
                    {
                        if (Semantics.FlipByProcess(before, child) == after)
                        {
                            if (!examples.ContainsKey(input))
                            {
                                examples[input] = new List<object>();
                            }

                            ((List<object>)examples[input]).Add(child);
                        }
                    }
                }
            }

            return new DisjunctiveExamplesSpec(examples);
        }
    }
}