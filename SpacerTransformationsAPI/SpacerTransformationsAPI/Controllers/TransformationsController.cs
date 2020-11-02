using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.AST;
using Microsoft.ProgramSynthesis.Compiler;
using Microsoft.ProgramSynthesis.Diagnostics;
using Microsoft.ProgramSynthesis.Learning;
using Microsoft.ProgramSynthesis.Learning.Logging;
using Microsoft.ProgramSynthesis.Learning.Strategies;
using Microsoft.ProgramSynthesis.VersionSpace;
using Microsoft.Z3;
using Newtonsoft.Json;
using SpacerTransformationsAPI.Functions;
using SpacerTransformationsAPI.Models;
using SpacerTransformationsAPI.Prose;

namespace SpacerTransformationsAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]/[action]")]
    public class TransformationsController : Controller
    {
        private const string PathToFiles = @"./";
        private const string SimpleBakeryPrefix = "(declare-const State (Array Int Int)) " +
                                                  "(declare-const Num (Array Int Int)) " +
                                                  "(declare-const Issue Int) " +
                                                  "(declare-const Serve Int) " +
                                                  "(assert {0})";
        private Result<Grammar> _grammar;
        private static SynthesisEngine _prose;

        [HttpPost]
        public async Task<ActionResult> LearnTransformation([FromBody]string instance)
        {
            try
            {
                Console.WriteLine(instance);
                const string grammarFileName = @"Prose/Transformations.grammar";

                var reader = new StreamReader(PathToFiles + grammarFileName);
                var grammar = await reader.ReadToEndAsync();
                
                _grammar = DSLCompiler.Compile(
                    new CompilerOptions()
                    {
                        InputGrammarText = grammar,
                        References = CompilerReference.FromAssemblyFiles(
                            typeof(List<int>).GetTypeInfo().Assembly,
                            typeof(Semantics).GetTypeInfo().Assembly,
                            typeof(Node).GetTypeInfo().Assembly)
                    }
                );

                var rawLemmas = await DynamoDb.GetLemmas(instance);
                var lemmas = DynamoDb.GetChangedLemmas(rawLemmas);
                ProgramSet learned;
                using (var ctx = new Context())
                {
                    var inputExamples = DynamoDb.GetInputOutputExamples(ctx, lemmas.Lemmas
                        .Select(kvp => kvp.Value).ToList(), SimpleBakeryPrefix);
                    var spec = Utils.CreateExampleSpec(_grammar, inputExamples);
                    RankingScore.ScoreForContext = 100;
                    var scoreFeature = new RankingScore(_grammar.Value);
                    DomainLearningLogic learningLogic = new WitnessFunctions(_grammar.Value);
                    _prose = new SynthesisEngine(_grammar.Value,
                        new SynthesisEngine.Config
                        {
                            LogListener = new LogListener(),
                            Strategies = new ISynthesisStrategy[] {new DeductiveSynthesis(learningLogic)},
                            UseThreads = false,
                            CacheSize = int.MaxValue
                        });
                    learned = _prose.LearnGrammarTopK(spec, scoreFeature);
                }
                return Ok(JsonConvert.SerializeObject(learned.RealizedPrograms.ToList()));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult> ApplyTransformation(string instance)
        {
            
                    Console.WriteLine("Possible Programs: " + learned);
                    finalProgram = learned.RealizedPrograms.First();
                    
                    foreach (var kvp in lemmas.Lemmas)
                    {
                        if (kvp.Value.Raw != "")
                        {
                            var parsedSmtLib =
                                SmtLib.StringToSmtLib(ctx, string.Format(SimpleBakeryPrefix, kvp.Value.Raw));
                            if (parsedSmtLib.FuncDecl.DeclKind == Z3_decl_kind.Z3_OP_OR)
                            {
                                var input = Utils.HandleSmtLibParsed(parsedSmtLib, ctx);
                                var stateInput = State.CreateForExecution(_grammar.Value.InputSymbol, input);
                                var result = (Node) finalProgram.Invoke(stateInput);
                                var lhs = (List<int>) finalProgram.Children[1].Invoke(stateInput);
                                lemmas.Lemmas[kvp.Key].Edited = ReadableParser.ParseResult(result.Expr, lhs.Count == input.Children.Count);
                                lemmas.Lemmas[kvp.Key].Lhs = lhs;
                            }

                        }
                    }
                    ctx.Dispose();
                }
                Console.WriteLine("Transformation complete");
        }
    }
}