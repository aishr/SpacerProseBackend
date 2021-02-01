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

                ProgramSet learned;
                using (var ctx = new Context())
                {
                    var inputExamples = DynamoDb.GetInputOutputExamplesModified(ctx, requestBody.InputOutputExamples,
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

                var rawLemmas = requestBody.ExprMap;
                var instance = requestBody.Instance;
                // var lemmas = DynamoDb.DbToSpacerInstance(instance, rawLemmas);
                var lemmas = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(rawLemmas);
                var exprMap = new SpacerInstance(instance);
                using (var ctx = new Context())
                {
                    foreach (var kvp in lemmas)
                    {
                        var raw = "";
                        var readable = "";
                        Console.WriteLine("kvp", kvp);
                        foreach(var lemma_kvp in kvp.Value){
                            if (lemma_kvp.Key == "raw"){
                                raw = (string)lemma_kvp.Value;
                            }
                            if(lemma_kvp.Key == "readable"){
                                readable = (string)lemma_kvp.Value;
                            }

                        }
                        if (raw != "")
                        {
                            try{
                                //add try catch to handle stuff that cannot be parsed
                                var parsedSmtLib =
                                    SmtLib.StringToSmtLib(ctx, string.Format(SmtPrefix, requestBody.DeclareStatements, raw));
                                if (parsedSmtLib.FuncDecl.DeclKind == Z3_decl_kind.Z3_OP_OR)
                                {
                                    var input = Utils.HandleSmtLibParsed(parsedSmtLib, ctx);
                                    var stateInput = State.CreateForExecution(_grammar.Value.InputSymbol, input);
                                    var result = (Node) finalProgram.Invoke(stateInput);

                                    exprMap.Lemmas.Add(int.Parse(kvp.Key), new Lemma(){
                                            Raw = result.Expr.ToString(),
                                                Readable = ReadableParser.ParseResult(result.Expr),
                                                }
                                    );
                                    Console.WriteLine(result.Expr);
                                }

                            }
                            catch (Exception ex){
                                //something wrong
                                Console.WriteLine(ex);
                                exprMap.Lemmas.Add(int.Parse(kvp.Key), new Lemma(){
                                        Raw = raw,
                                            Readable = readable,
                                            }
                                );

                            }

                        }
                    }
                    ctx.Dispose();
                }
                Console.WriteLine("Transformation complete");
                return Ok(JsonConvert.SerializeObject(exprMap.Lemmas));
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
