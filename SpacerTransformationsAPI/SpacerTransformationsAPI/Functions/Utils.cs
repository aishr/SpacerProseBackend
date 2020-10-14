using System;
using System.Collections.Generic;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.Diagnostics;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.Z3;
using SpacerTransformationsAPI.Models;

namespace SpacerTransformationsAPI.Functions
{
    public static class Utils
    {
        public static Node HandleSmtLibParsed(Expr input, Context ctx)
        {
            var result = new Node(input.FuncDecl.DeclKind, new List<Node>(), input, ctx);
            if (input.NumArgs == 0)
            {
                return result;
            }
            foreach (var expr in input.Args)
            {
                result.AddChild(HandleSmtLibParsed(expr, ctx));
            }

            return result;
        }

        public static Expr HandleNegation(Expr input, Context ctx)
        {
            var type = input.FuncDecl.DeclKind;

            switch (type)
            {
                case Z3_decl_kind.Z3_OP_LE:
                    return ctx.MkGt((ArithExpr)input.Args[0], (ArithExpr)input.Args[1]);
                case Z3_decl_kind.Z3_OP_LT:
                    return ctx.MkGe((ArithExpr)input.Args[0], (ArithExpr)input.Args[1]);
                case Z3_decl_kind.Z3_OP_GE:
                    return ctx.MkLt((ArithExpr)input.Args[0], (ArithExpr)input.Args[1]);
                case Z3_decl_kind.Z3_OP_GT:
                    return ctx.MkLe((ArithExpr)input.Args[0], (ArithExpr)input.Args[1]);
                case Z3_decl_kind.Z3_OP_NOT:
                    return input.Args[0];
                default:
                    return ctx.MkNot((BoolExpr) input);
            }
        }
        public static ExampleSpec CreateExampleSpec(Result<Grammar> grammar, IEnumerable<Tuple<Node, Node>> examples)
        {
            var proseExamples = new Dictionary<State, object>();
            foreach (var example in examples)
            {
                var input = State.CreateForExecution(grammar.Value.InputSymbol, example.Item1);
                var astAfter = example.Item2;
                proseExamples.Add(input, astAfter);
            }
            var spec = new ExampleSpec(proseExamples);
            return spec;
        }
        
    }
}