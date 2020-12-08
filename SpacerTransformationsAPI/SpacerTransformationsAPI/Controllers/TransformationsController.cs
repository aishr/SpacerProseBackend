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
        private const string SmtPrefix = "{0} (assert {1})";
        private Result<Grammar> _grammar;
        private static SynthesisEngine _prose;

        public TransformationsController()
        {
                const string grammarFileName = @"Prose/Transformations.grammar";

                var reader = new StreamReader(PathToFiles + grammarFileName);
                var grammar = reader.ReadToEnd();
                
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
        }
        
        
        [HttpPost]
        public ActionResult LearnTransformationModified([FromBody]LearnTransformModifiedRequestBody requestBody)
        {
            try
            {
                Console.WriteLine(requestBody.Instance);

                ProgramSet learned;
                using (var ctx = new Context())
                {
                    var inputExamples = DynamoDb.GetInputOutputExamplesModified(ctx, requestBody.InputOutputExamples,
                        SmtPrefix, requestBody.DeclareStatements);
                    Console.WriteLine("Debug: pulled from dynamodb");
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

                var finalPrograms = learned.RealizedPrograms.Select(program => new FinalProgram(program.ToString(), program.PrintAST())).ToList();
                return Ok(JsonConvert.SerializeObject(finalPrograms));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> LearnTransformation([FromBody]LearnTransformRequestBody requestBody)
        {
            try
            {
                Console.WriteLine(requestBody.Instance);

                var rawLemmas = await DynamoDb.GetLemmas(requestBody.Instance);
                Console.WriteLine("Debug: pulled from dynamodb");
                var lemmas = DynamoDb.GetChangedLemmas(rawLemmas);
                ProgramSet learned;
                using (var ctx = new Context())
                {
                    var inputExamples = DynamoDb.GetInputOutputExamples(ctx, lemmas.Lemmas
                        .Select(kvp => kvp.Value).ToList(), SmtPrefix, requestBody.DeclareStatements);
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

                var finalPrograms = learned.RealizedPrograms.Select(program => new FinalProgram(program.ToString(), program.PrintAST())).ToList();
                return Ok(JsonConvert.SerializeObject(finalPrograms));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> ApplyTransformation([FromBody]ApplyTransformRequestBody requestBody)
        {
            try
            {
                Console.WriteLine(requestBody.Instance);
                Console.WriteLine(requestBody.Program);
                var finalProgram = ProgramNode.Parse(requestBody.Program, _grammar.Value);

                var rawLemmas = await DynamoDb.GetLemmas(requestBody.Instance);
                var lemmas = DynamoDb.DbToSpacerInstance(rawLemmas);
                using (var ctx = new Context())
                {
                    foreach (var kvp in lemmas.Lemmas)
                    {
                        if (kvp.Value.Raw != "")
                        {
                            var parsedSmtLib =
                                SmtLib.StringToSmtLib(ctx, string.Format(SmtPrefix, requestBody.DeclareStatements, kvp.Value.Raw));
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
                return Ok(JsonConvert.SerializeObject(lemmas.Lemmas));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
    }
}