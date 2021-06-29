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
        //Given inputTree, I and outputTree, O, WitnessLeftSide(I, O) = R
        //where for every r in R, Transform2(I, r) = O
        // [a, b, c] -> [a], [b], [c], [a,b], [a,c], [a,b,c]
        [WitnessFunction(nameof(Semantics.ToImp), 1)]
        public ExampleSpec WitnessToImpLeftSide(GrammarRule rule, ExampleSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 1");
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
                    if (Semantics.ToImp(before, subList).IsEqualTo(after))
                    {
                        examples[input] = subList;
                    }
                }
                
            }
            return new ExampleSpec(examples);
        }

        [WitnessFunction(nameof(Semantics.JoinFilters), 0, Verify = true)]
        public DisjunctiveExamplesSpec WitnessJoinFiltersFilterOne(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 0");
            var examples = new Dictionary<State, IEnumerable<object>>();
            foreach (var input in spec.ProvidedInputs)
            {
                var after = (List<int>) spec.DisjunctiveExamples[input].First();
                if (after.Count() < 2)
                {
                    return null;
                }
                if (!examples.ContainsKey(input))
                {
                    examples[input] = new List<IEnumerable<int>>();
                }

                for (var i = 1; i < after.Count; i++)
                {
                    var possibleArrays = Utils.GetKCombs(after, i);
                    var possibleLists = possibleArrays.Select(x => x.ToList());
                    examples[input] = ((List<IEnumerable<int>>) examples[input]).Concat(possibleLists);

                }
                
            }

            return new DisjunctiveExamplesSpec(examples);
        }

        [WitnessFunction(nameof(Semantics.JoinFilters), 1, DependsOnParameters = new [] {0})]
        public DisjunctiveExamplesSpec WitnessJoinFiltersFilterTwo(GrammarRule rule, DisjunctiveExamplesSpec spec, DisjunctiveExamplesSpec filterOneSpec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 1");
            var examples = new Dictionary<State, object>();
            foreach (var input in spec.ProvidedInputs)
            {
                var before = (IEnumerable<int>) input.Bindings.First().Value;
                //var after = (IEnumerable<int>) spec.Examples[input];
            }

            return new ExampleSpec(examples);
            
        }


        //IEnumerable<int> FilterByName(Node inputTree, string name)
        [WitnessFunction(nameof(Semantics.FilterByName), 1)]
        public DisjunctiveExamplesSpec WitnessFilterByNameName(GrammarRule rule, ExampleSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 1");
            var examples = new Dictionary<State, IEnumerable<object>>();
            foreach (var input in spec.ProvidedInputs)
            {
                var before = (Node) input[rule.Body[0]];
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
        
        //IEnumerable<int> FilterByProcess(Node inputTree, string process)
        [WitnessFunction(nameof(Semantics.FilterByArrayIndex), 1)]
        public DisjunctiveExamplesSpec WitnessFilterByProcessProcess(GrammarRule rule, ExampleSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 1");
            var examples = new Dictionary<State, IEnumerable<object>>();
            foreach (var input in spec.ProvidedInputs)
            {
                var before = (Node)input[rule.Body[0]];
                var after = (List<int>)spec.Examples[input];
                var children = before.GetProcesses();    
                foreach (var child in children)
                {
                    if (Semantics.FilterByArrayIndex(before, child).OrderBy(i => i)
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
        
        
        //IEnumerable<int> FilterStatic(Node inputTree, StaticFilterType type)
        [WitnessFunction(nameof(Semantics.FilterStatic), 1)]
        public DisjunctiveExamplesSpec WitnessFilterStaticType(GrammarRule rule, ExampleSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 1");
            var examples = new Dictionary<State, IEnumerable<object>>();
            foreach (var input in spec.ProvidedInputs)
            {
                var before = (Node)input[rule.Body[0]];
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
    }
}
