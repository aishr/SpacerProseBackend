using System.Collections.Generic;
using System.Linq;
using Microsoft.Z3;
using SpacerTransformationsAPI.Functions;
using SpacerTransformationsAPI.Models;

namespace SpacerTransformationsAPI.Prose
{
    public static class Semantics
    {
        public static Node ToImp(Node inputTree, List<int> leftSide)
        {
            if (inputTree.Expr.FuncDecl.DeclKind != Z3_decl_kind.Z3_OP_OR)
            {
                return inputTree;
            }
            
            var ctx = inputTree.Ctx;
            var children = inputTree.Children;
            var leftSideNodes = new List<BoolExpr>();
            var rightSideNodes = new List<BoolExpr>();
            
            for (var i = 0; i < children.Count; i++)
            {
                if (leftSide.Contains(i))
                {
                    leftSideNodes.Add((BoolExpr)Utils.HandleNegation(children[i].Expr, ctx));
                }
                else
                {
                    rightSideNodes.Add((BoolExpr)children[i].Expr);
                }
            }

            var impliesLeftSide = leftSideNodes.Count == 1 ? leftSideNodes[0] : ctx.MkAnd(leftSideNodes);
            var impliesRightSide = rightSideNodes.Count == 1 ? rightSideNodes[0] : ctx.MkOr(rightSideNodes);
            
            if (leftSideNodes.Count == 0)
            {
                return Utils.HandleSmtLibParsed(impliesRightSide, ctx);
            }
            
            if (rightSideNodes.Count == 0)
            {
                return Utils.HandleSmtLibParsed(ctx.MkImplies(impliesLeftSide, ctx.MkBool(false)), ctx);
            }

            var result = ctx.MkImplies(impliesLeftSide, impliesRightSide);
            return Utils.HandleSmtLibParsed(result, ctx);
        }

        public static IEnumerable<int> JoinFilters(IEnumerable<int> filter1, IEnumerable<int> filter2)
        {
            return filter1.Union(filter2);
        }

        public static IEnumerable<int> FilterByName(Node inputTree, string name)
        {
            var result = new List<int>();
            var children = inputTree.Children;
            
            for (var i = 0; i < children.Count; i++)
            {
                if (children[i].HasIdentifier(name))
                {
                    result.Add(i);
                }
            }

            return result;
        }
        
        public static IEnumerable<int> FilterByArrayIndex(Node inputTree, string index)
        {
            var result = new List<int>();
            var children = inputTree.Children;
            for (var i = 0; i < children.Count; i++)
            {
                var exprs = children[i].FlattenTree();
                var indices = Utils.FindSelect(exprs.ToList());

                foreach(int ind in indices)
                {
                    var selectExpr = exprs[ind];

                    if (selectExpr.Args[1].ToString() == index)
                    {
                        result.Add(i);
                    }
                }          
            }

            return result;
        }

        public static IEnumerable<int> FilterStatic(Node inputTree, StaticFilterType type)
        {
            var children = inputTree.Children;

            switch (type)
            {
                case StaticFilterType.Not:
                    var result = new List<int>();

                    for (var i = 0; i < children.Count; i++)
                    {
                        if (children[i].IsNot())
                        {
                            result.Add(i);
                        }
                    }
                    return result;
                
                case StaticFilterType.AllButLast:
                    return Enumerable.Range(0, children.Count - 1).ToList();
                
                default:
                    return null;
            }
        }
    }
}
