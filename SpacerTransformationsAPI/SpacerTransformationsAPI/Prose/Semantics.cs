using System.Collections.Generic;
using System.Linq;
using Microsoft.ProgramSynthesis.Split.Text.Build.RuleNodeTypes;
using Microsoft.ProgramSynthesis.Transformation.Text.Build.NodeTypes;
using Microsoft.Z3;
using SpacerTransformationsAPI.Functions;
using SpacerTransformationsAPI.Models;

namespace SpacerTransformationsAPI.Prose
{
    public static class Semantics
    {
        public static Node Transform(Node inputTree, List<int> leftSide)
        {
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
                return Utils.HandleSmtLibParsed(impliesLeftSide, ctx);
            }

            var result = ctx.MkImplies(impliesLeftSide, impliesRightSide);
            return Utils.HandleSmtLibParsed(result, ctx);
        }
        public static IEnumerable<int> Filter(Node inputTree, string name)
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

        public static IEnumerable<int> FilterAllButLast(Node inputTree)
        {
            var children = inputTree.Children;
            
            return Enumerable.Range(0, children.Count - 1).ToList();
        }
        
        public static IEnumerable<int> FilterByProcess(Node inputTree, string process)
        {
            var result = new List<int>();
            var children = inputTree.Children;
            for (var i = 0; i < children.Count; i++)
            {
                var exprs = children[i].FlattenTree();
                var indices = Utils.FindSelect(exprs.ToList());

                foreach(int index in indices)
                {
                    var selectExpr = exprs[index];

                    if (selectExpr.Args[1].ToString() == process)
                    {
                        result.Add(i);
                    }
                }          
            }

            return result;
        }

        public static IEnumerable<int> FilterByNot(Node inputTree)
        {
            var result = new List<int>();
            var children = inputTree.Children;

            for (var i = 0; i < children.Count; i++)
            {
                if (children[i].IsNot())
                {
                    result.Add(i);
                }
            }

            return result;
        }
    }
}