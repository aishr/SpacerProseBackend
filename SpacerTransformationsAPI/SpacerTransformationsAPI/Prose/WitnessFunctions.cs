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
        
        //Node Transform(Node inputTree, List<int> leftSide)
        //Given inputTree, I and outputTree, O, WitnessLeftSide(I, O) = R
        //where for every r in R, Transform2(I, r) = O
        // [a, b, c] -> [a], [b], [c], [a,b], [a,c], [a,b,c]
        [WitnessFunction("Transform", 1)]
        public ExampleSpec WitnessLeftSide(GrammarRule rule, ExampleSpec spec)
        {
            var examples = new Dictionary<State, object>();
            foreach (var input in spec.ProvidedInputs)
            {
                var before = (Node) input[rule.Body[0]];
                var after = (Node) spec.Examples[input];
                var children = before.Children;
                var numLeftSide = after.Type == Z3_decl_kind.Z3_OP_AND ? after.Children.Count : after.Children[0].Type == Z3_decl_kind.Z3_OP_AND ? after.Children[0].Children.Count : 1;
                var listOfIndices = Enumerable.Range(0, children.Count);
                var possibleLists = Utils.GetKCombs(listOfIndices, numLeftSide);
                foreach (var sub in possibleLists)
                {
                    var subList = sub.ToList();
                    if (Semantics.Transform(before, subList).IsEqualTo(after))
                    {
                        examples[input] = subList;
                    }
                }
                
            }
            return new ExampleSpec(examples);
        }
        
        
        //List<int> Filter(Node inputTree, string name)
        [WitnessFunction("Filter", 1)]
        public DisjunctiveExamplesSpec WitnessName3(GrammarRule rule, ExampleSpec spec)
        {
            var examples = new Dictionary<State, IEnumerable<object>>();
            foreach (var input in spec.ProvidedInputs)
            {
                var before = (Node) input[rule.Body[0]];
                var after = (List<int>) spec.Examples[input];
                var children = before.GetIdentifiers();
                foreach (var child in children)
                {
                    if (Semantics.Filter(before, child).OrderBy(i => i)
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
        
        
        //List<int> FilterByProcess(Node inputTree, string process)
        [WitnessFunction("FilterByProcess", 1)]
        public DisjunctiveExamplesSpec WitnessProcess(GrammarRule rule, ExampleSpec spec)
        {
            var examples = new Dictionary<State, IEnumerable<object>>();
            foreach (var input in spec.ProvidedInputs)
            {
                var before = (Node)input[rule.Body[0]];
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
        
        
        //IEnumerable<int> FilterAllButLast(Node inputTree, string temp)
        [WitnessFunction("FilterAllButLast", 1)]
        public ExampleSpec WitnessFilterAllButLast(GrammarRule rule, ExampleSpec spec)
        {
            var examples = new Dictionary<State, object>();
            foreach (var input in spec.ProvidedInputs)
            {
                var before = (Node)input[rule.Body[0]];
                var after = (List<int>)spec.Examples[input];

                if (Semantics.FilterAllButLast(before, "temp").OrderBy(i => i)
                    .SequenceEqual(after.OrderBy(i => i)))
                {
                    examples[input] = "temp";
                }
            }

            return new ExampleSpec(examples);
        }

        //IEnumerable<int> FilterByNot(Node inputTree, string temp)
        [WitnessFunction("FilterByNot", 1)]
        public ExampleSpec WitnessFilterByNot(GrammarRule rule, ExampleSpec spec)
        {
            var examples = new Dictionary<State, object>();
            foreach (var input in spec.ProvidedInputs)
            {
                var before = (Node)input[rule.Body[0]];
                var after = (List<int>)spec.Examples[input];

                if (Semantics.FilterByNot(before, "temp").OrderBy(i => i)
                    .SequenceEqual(after.OrderBy(i => i)))
                {
                    examples[input] = "temp";
                }
            }

            return new ExampleSpec(examples);
        }
        
        //Node Move(Node inputTree, Tuple<int, bool> positionLeft)
        [WitnessFunction("Move", 1)]
        public ExampleSpec WitnessMovePosition(GrammarRule rule, ExampleSpec spec)
        {
            var examples = new Dictionary<State, object>();
            foreach (var input in spec.ProvidedInputs)
            {
                var before = (Node)input[rule.Body[0]];
                var after = (Node)spec.Examples[input];
                for (var i = 0; i < after.Children.Count; ++i)
                {
                    if (Semantics.Move(before, new Tuple<int, bool>(i, true)).IsEqualTo(after))
                    {
                        examples[input] = new Tuple<int, bool>(i, true);
                    }
                    if (Semantics.Move(before, new Tuple<int, bool>(i, false)).IsEqualTo(after))
                    {
                        examples[input] = new Tuple<int, bool>(i, false);
                    }
                }
            }

            return new ExampleSpec(examples);
        }

        //string IndexByName(Node inputTree, string name)
        [WitnessFunction("IndexByName", 1)]
        public ExampleSpec WitnessIndexByName(GrammarRule rule, ExampleSpec spec)
        {
            var examples = new Dictionary<State, object>();
            foreach (var input in spec.ProvidedInputs)
            {
                var before = (Node)input[rule.Body[0]];
                var after = (string)spec.Examples[input];
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

        //string IndexFromFront(Node inputTree, string index)
        [WitnessFunction("IndexFromFront", 1)]
        public ExampleSpec WitnessIndexByIndex(GrammarRule rule, ExampleSpec spec)
        {
            var examples = new Dictionary<State, object>();
            foreach (var input in spec.ProvidedInputs)
            {
                var before = (Node)input[rule.Body[0]];
                var after = (string)spec.Examples[input];

                for (var i = 0; i < before.Children.Count; ++i)
                {
                    if(Semantics.IndexFromFront(before, i.ToString()) == after)
                    {
                        examples[input] = i.ToString();
                    }
                }
            }

            return new ExampleSpec(examples);
        }

        //string IndexFromBack(Node inputTree, string index)
        [WitnessFunction("IndexFromBack", 1)]
        public ExampleSpec WitnessIndexLast(GrammarRule rule, ExampleSpec spec)
        {
            var examples = new Dictionary<State, object>();
            foreach (var input in spec.ProvidedInputs)
            {
                var before = (Node)input[rule.Body[0]];
                var after = (string)spec.Examples[input];

                for (var i = 0; i < before.Children.Count; ++i)
                {
                    if (Semantics.IndexFromBack(before, i.ToString()) == after)
                    {
                        examples[input] = i.ToString();
                    }
                }
            }

            return new ExampleSpec(examples);
        }

        //Tuple<int, bool> MakeMoveLeft(Node inputTree, string position)
        [WitnessFunction("MakeMoveLeft", 1)]
        public ExampleSpec MakeMoveLeft(GrammarRule rule, ExampleSpec spec)
        {
            var examples = new Dictionary<State, object>();
            foreach (var input in spec.ProvidedInputs)
            {
                var before = (Node)input[rule.Body[0]];
                var after = (Tuple<int, bool>)spec.Examples[input];

                for (var i = 0; i < before.Children.Count; ++i)
                {
                    if (Semantics.MakeMoveLeft(before, i.ToString()).Equals(after))
                    {
                        examples[input] = i.ToString();
                    }
                }
            }

            return new ExampleSpec(examples);
        }

        //Tuple<int, bool> MakeMoveRight(Node inputTree, string position)
        [WitnessFunction("MakeMoveRight", 1)]
        public ExampleSpec MakeMoveRight(GrammarRule rule, ExampleSpec spec)
        {
            var examples = new Dictionary<State, object>();
            foreach (var input in spec.ProvidedInputs)
            {
                var before = (Node)input[rule.Body[0]];
                var after = (Tuple<int, bool>)spec.Examples[input];

                for (var i = 0; i < before.Children.Count; ++i)
                {
                    if (Semantics.MakeMoveRight(before, i.ToString()).Equals(after))
                    {
                        examples[input] = i.ToString();
                    }
                }
            }

            return new ExampleSpec(examples);
        }

        //Node SquashNegation(Node inputTree, string temp)
        [WitnessFunction("SquashNegation", 1)]
        public ExampleSpec WitnessSquashNegation(GrammarRule rule, ExampleSpec spec)
        {
            var examples = new Dictionary<State, object>();
            foreach (var input in spec.ProvidedInputs)
            {
                var before = (Node)input[rule.Body[0]];
                var after = (Node)spec.Examples[input];

                if(Semantics.SquashNegation(before, "temp").IsEqualTo(after))
                {
                    examples[input] = "temp";
                }
            }

            return new ExampleSpec(examples);
        }

        //Node SquashNegation(Node inputTree, string temp)
        [WitnessFunction("FlipComparison", 1)]
        public ExampleSpec WitnessFlipComparison(GrammarRule rule, ExampleSpec spec)
        {
            var examples = new Dictionary<State, object>();
            foreach (var input in spec.ProvidedInputs)
            {
                var before = (Node)input[rule.Body[0]];
                var after = (Node)spec.Examples[input];

                if (Semantics.FlipComparison(before, "temp").IsEqualTo(after))
                {
                    examples[input] = "temp";
                }
            }

            return new ExampleSpec(examples);
        }
    }
}