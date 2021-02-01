using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Z3;
using SpacerTransformationsAPI.Models;
using Newtonsoft.Json;


namespace SpacerTransformationsAPI.Functions
{
    public static class DynamoDb
    {
        public static IEnumerable<Tuple<Node, Node>> GetInputOutputExamplesModified(Context ctx,
            List<TrainingInputOutput> trainingExamples, string prefix, string declareStatements)
        {
            var results = new List<Tuple<Node, Node>>();
             foreach (var example in trainingExamples)
             {
                 Console.WriteLine(example);
                 var input = SmtLib.StringToSmtLib(ctx, string.Format(prefix, declareStatements, example.Input));
                 var inputTree = Utils.HandleSmtLibParsed(input, ctx);
                 var output = SmtLib.StringToSmtLib(ctx, string.Format(prefix, declareStatements, example.Output));
                 var outputTree = Utils.HandleSmtLibParsed(output, ctx);
                 results.Add(new Tuple<Node, Node>(inputTree, outputTree));
             }

             return results;
        }
    }
}
