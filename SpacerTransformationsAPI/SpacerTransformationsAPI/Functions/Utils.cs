using System;
using System.Collections.Generic;
using System.Linq;
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
        
        public static IEnumerable<IEnumerable<T>> GetKCombs<T>(IEnumerable<T> list, int length) where T : IComparable
        {
            if (length == 1) return list.Select(t => new[] { t });
            return GetKCombs(list, length - 1)
                .SelectMany(t => list.Where(o => o.CompareTo(t.Last()) > 0), 
                    (t1, t2) => t1.Concat(new[] { t2 }));
        }
        
        public static IEnumerable<int> FindSelect(List<Expr> exprs)
        {
            var indices = new List<int>();

            for (var i = 0; i < exprs.Count; ++i)
            {
                if (exprs[i].FuncDecl.DeclKind == Z3_decl_kind.Z3_OP_SELECT)
                {
                    indices.Add(i);
                }
            }

            return indices;
        }
    }
}