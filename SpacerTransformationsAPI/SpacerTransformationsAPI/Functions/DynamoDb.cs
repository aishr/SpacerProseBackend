using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Microsoft.Z3;
using SpacerTransformationsAPI.Models;
using SpacerTransformationsAPI.Prose;

namespace SpacerTransformationsAPI.Functions
{
    public static class DynamoDb
    {
        private static readonly AmazonDynamoDBClient Client = new AmazonDynamoDBClient(
            new BasicAWSCredentials(Environment.GetEnvironmentVariable("ACCESS_KEY_ID"),
                Environment.GetEnvironmentVariable("SECRET_ACCESS_KEY")), 
            RegionEndpoint.USEast2);
        
        public static async Task<GetItemResponse> GetLemmas(string id)
        {
            var key = new Dictionary<string, AttributeValue>()
            {
                {"Id", new AttributeValue(id)}
            };
            var request = new GetItemRequest(Environment.GetEnvironmentVariable("TABLE_NAME"), key);

            return await Client.GetItemAsync(request);
        }

        public static SpacerInstance GetChangedLemmas(GetItemResponse response)
        {
            var results = new SpacerInstance(response.Item["Id"].S);
            foreach (var lemmaNum in response.Item)
            {
                if (lemmaNum.Key == "Id") continue;
                var potentialList = lemmaNum.Value.M["lhs"].L;
                if (lemmaNum.Value.M["changed"].BOOL)
                {
                    results.Lemmas.Add(int.Parse(lemmaNum.Key), CreateLemma(lemmaNum.Value, potentialList));
                }
            }

            return results;
        }

        public static SpacerInstance DbToSpacerInstance(GetItemResponse response)
        {
            var results = new SpacerInstance(response.Item["Id"].S);
            foreach (var lemmaNum in response.Item)
            {
                if (lemmaNum.Key == "Id") continue;
                var potentialList = lemmaNum.Value.M["lhs"].L;
                results.Lemmas.Add(int.Parse(lemmaNum.Key), CreateLemma(lemmaNum.Value, potentialList));
            }
            return results;
        }

        private static Lemma CreateLemma(AttributeValue itemValue, List<AttributeValue> potentialList)
        {
            return new Lemma() 
            {
                 Edited = itemValue.M["edited"].S,
                 Readable = itemValue.M["readable"].S,
                 Raw = itemValue.M["raw"].S,
                 Lhs = potentialList.Count == 0 ? new List<int>() : potentialList.Select(x => int.Parse(x.N)).ToList(),
                 Changed = itemValue.M["changed"].BOOL
            };
            
        }

        public static IEnumerable<Tuple<Node, Node>> GetInputOutputExamples(Context ctx, IEnumerable<Lemma> lemmas, string prefix, string declareStatements)
        {
            var results = new List<Tuple<Node, Node>>();
             foreach (var lemma in lemmas)
             {
                 if (!lemma.Changed) continue;
                 var input = SmtLib.StringToSmtLib(ctx, string.Format(prefix, declareStatements, lemma.Raw));
                 var inputTree = Utils.HandleSmtLibParsed(input, ctx);
                 var outputTree = Semantics.Transform(inputTree, lemma.Lhs);
                 results.Add(new Tuple<Node, Node>(inputTree, outputTree));
             }

             return results;
        }

        public static IEnumerable<Tuple<Node, Node>> GetInputOutputExamplesModified(Context ctx,
            List<TrainingInputOutput> trainingExamples, string prefix, string declareStatements)
        {
            var results = new List<Tuple<Node, Node>>();
             foreach (var example in trainingExamples)
             {
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