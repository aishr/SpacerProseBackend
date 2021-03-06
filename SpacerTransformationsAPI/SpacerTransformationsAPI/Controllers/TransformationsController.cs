using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        private readonly Result<Grammar> _grammar;
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
        public ActionResult LearnTransformation([FromBody]LearnTransformRequestBody requestBody)
        {
            try
            {
                Console.WriteLine(requestBody.Instance);

                ProgramSet learned;
                using (var ctx = new Context())
                {
                    var inputExamples = Utils.GetInputOutputExamplesModified(ctx, requestBody.InputOutputExamples,
                        SmtPrefix, requestBody.DeclareStatements);
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
                if (finalPrograms.Count == 0) {
                    Console.WriteLine("No Programs Found");
                }

                foreach (var program in finalPrograms)
                {
                    Console.WriteLine(program.HumanReadableAst);
                }
                return Ok(JsonConvert.SerializeObject(finalPrograms));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(ex.TargetSite);
                Console.WriteLine("Error: " + ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
        
        [HttpPost]
        public ActionResult ApplyTransformation([FromBody]ApplyTransformRequestBody requestBody)
        {
            try
            {
                Console.WriteLine(requestBody.Instance);
                Console.WriteLine(requestBody.Program);
                var finalProgram = ProgramNode.Parse(requestBody.Program, _grammar.Value);

                var rawSpacerInstance = requestBody.SpacerInstance;
                var lemmas = JsonConvert.DeserializeObject<SpacerInstance>(rawSpacerInstance);
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

                                lemmas.Lemmas[kvp.Key].Raw = result.Expr.ToString();
                                lemmas.Lemmas[kvp.Key].Readable = ReadableParser.ParseResult(result.Expr);
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
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(ex.TargetSite);
                Console.WriteLine("Error: " + ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
        
        [HttpPost]
        public ActionResult GetReadable([FromBody] ReadableRequestBody requestBody)
        {
            
            try
            {
                Console.WriteLine(requestBody.Instance);

                var rawSpacerInstance = requestBody.SpacerInstance;
                var lemmas = JsonConvert.DeserializeObject<SpacerInstance>(rawSpacerInstance);
                using (var ctx = new Context())
                {
                    foreach (var kvp in lemmas.Lemmas)
                    {
                        if (kvp.Value.Raw == "") continue;
                        var parsedSmtLib =
                            SmtLib.StringToSmtLib(ctx, string.Format(SmtPrefix, requestBody.DeclareStatements, kvp.Value.Raw));

                        lemmas.Lemmas[kvp.Key].Readable = ReadableParser.ParseResult(parsedSmtLib);
                    }
                    ctx.Dispose();
                }
                Console.WriteLine("Readable complete");
                return Ok(JsonConvert.SerializeObject(lemmas.Lemmas));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(ex.TargetSite);
                Console.WriteLine("Error: " + ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
    }
}
