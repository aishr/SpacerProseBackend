using System;
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
        
        public static Node Move(Node inputTree, Tuple<int, bool> positionLeft)
        {
            var ctx = inputTree.Ctx;
            var children = inputTree.Children;
            var retExprs = new List<Expr>();
            var op = inputTree.Expr.FuncDecl.DeclKind;
            var movable = new List<Z3_decl_kind>()
            {
                Z3_decl_kind.Z3_OP_ADD,
                Z3_decl_kind.Z3_OP_MUL,
                Z3_decl_kind.Z3_OP_EQ,
                Z3_decl_kind.Z3_OP_AND,
                Z3_decl_kind.Z3_OP_OR
            };

            if(movable.Contains(op) && positionLeft.Item1 >= 0)
            {
                foreach (var child in children)
                {
                    retExprs.Add(child.Expr);
                }

                for (var i = 0; i < retExprs.Count; ++i)
                {
                    if (i == positionLeft.Item1)
                    {
                        if (positionLeft.Item2 && i != 0)
                        {
                            var temp = retExprs[i];
                            retExprs[i] = retExprs[i - 1];
                            retExprs[i - 1] = temp;
                        }

                        else if (!positionLeft.Item2 && i != children.Count - 1)
                        {
                            var temp = retExprs[i];
                            retExprs[i] = retExprs[i + 1];
                            retExprs[i + 1] = temp;
                        }
                    }
                }
            }

            Expr result = null;

            switch (op)
            {
                case Z3_decl_kind.Z3_OP_ADD:
                    result = ctx.MkAdd(retExprs.Select(expr => (ArithExpr)expr));
                    break;
                case Z3_decl_kind.Z3_OP_MUL:
                    result = ctx.MkMul(retExprs.Select(expr => (ArithExpr)expr));
                    break;
                case Z3_decl_kind.Z3_OP_EQ:
                    result = ctx.MkEq(retExprs[0], retExprs[1]);
                    break;
                case Z3_decl_kind.Z3_OP_AND:
                    result = ctx.MkAnd(retExprs.Select(expr => (BoolExpr)expr));
                    break;
                case Z3_decl_kind.Z3_OP_OR:
                    result = ctx.MkOr(retExprs.Select(expr => (BoolExpr)expr));
                    break;
            }

            return Utils.HandleSmtLibParsed(result, ctx);
        }

        public static string IndexByName(Node inputTree, string name)
        {
            for (var i = 0; i < inputTree.Children.Count; ++i)
            {
                if (inputTree.Children[i].HasIdentifier(name))
                {
                    return i.ToString();
                }
            }

            return (-1).ToString();
        }

        public static string IndexFromFront(Node inputTree, string index)
        {
            return index;
        }

        public static string IndexFromBack(Node inputTree, string index)
        {
            return (inputTree.Children.Count - int.Parse(index) - 1).ToString();
        }

        public static Tuple<int, bool> MakeMoveLeft(Node inputTree, string position)
        {
            return new Tuple<int, bool>(int.Parse(position), true);
        }

        public static Tuple<int, bool> MakeMoveRight(Node inputTree, string position)
        {
            return new Tuple<int, bool>(int.Parse(position), false);
        }
    }
}