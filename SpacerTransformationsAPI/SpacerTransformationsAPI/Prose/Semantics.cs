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
                return Utils.HandleSmtLibParsed(impliesLeftSide, ctx);
            }

            var result = ctx.MkImplies(impliesLeftSide, impliesRightSide);
            return Utils.HandleSmtLibParsed(result, ctx);
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

        public static Node Move(Node inputTree, int position, bool left)
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

            if (!movable.Contains(op))
            {
                return inputTree;
            }

            if (!(position >= 0 && position < children.Count()))
            {
                return inputTree;
            }

            foreach (var child in children)
            {
                retExprs.Add(child.Expr);
            }

            for (var i = 0; i < retExprs.Count; ++i)
            {
                if (i == position)
                {
                    if (left && i != 0)
                    {
                        var temp = retExprs[i];
                        retExprs[i] = retExprs[i - 1];
                        retExprs[i - 1] = temp;
                    }

                    else if (!left && i != children.Count - 1)
                    {
                        var temp = retExprs[i];
                        retExprs[i] = retExprs[i + 1];
                        retExprs[i + 1] = temp;
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

        public static int IndexByName(Node inputTree, string name)
        {
            for (var i = 0; i < inputTree.Children.Count; ++i)
            {
                if (inputTree.Children[i].HasIdentifier(name))
                {
                    return i;
                }
            }

            return (-1);
        }

        public static int IndexFromFront(Node inputTree, int index)
        {
            return index;
        }

        public static int IndexFromBack(Node inputTree, int index)
        {
            return (inputTree.Children.Count - index - 1);
        }
        
        public static Node SquashNegation(Node inputTree, string symbol)
        {
            if (!inputTree.IsNot())
            {
                return inputTree;
            }

            // !(!(a = b)) --> a = b
            if(inputTree.Children[0].IsNot() && symbol == "not")
            {
                return inputTree.Children[0].Children[0];
            }

            // !(a > b) --> a <= b
            // !(a < b) --> a >= b
            else
            {
                var ctx = inputTree.Ctx;
                var children = inputTree.Children[0].Children;
                var op = inputTree.Children[0].Expr.FuncDecl;
                var negatable = new List<Z3_decl_kind>()
                {
                    Z3_decl_kind.Z3_OP_LT,
                    Z3_decl_kind.Z3_OP_GT,
                    Z3_decl_kind.Z3_OP_LE,
                    Z3_decl_kind.Z3_OP_GE,
                };

                if (!(negatable.Contains(op.DeclKind) && op.Name.ToString() == symbol))
                {
                    return inputTree;
                }

                Expr result = null;

                switch (op.DeclKind)
                {
                    case Z3_decl_kind.Z3_OP_LT:
                        result = ctx.MkGe((ArithExpr)children[0].Expr, (ArithExpr)children[1].Expr);
                        break;
                    case Z3_decl_kind.Z3_OP_GT:
                        result = ctx.MkLe((ArithExpr)children[0].Expr, (ArithExpr)children[1].Expr);
                        break;
                    case Z3_decl_kind.Z3_OP_LE:
                        result = ctx.MkGt((ArithExpr)children[0].Expr, (ArithExpr)children[1].Expr);
                        break;
                    case Z3_decl_kind.Z3_OP_GE:
                        result = ctx.MkLt((ArithExpr)children[0].Expr, (ArithExpr)children[1].Expr);
                        break;
                }

                return Utils.HandleSmtLibParsed(result, ctx);
            }
            
        }

        public static Node FlipComparison(Node inputTree, string symbol, string name)
        {
            var ctx = inputTree.Ctx;
            var children = inputTree.Children;
            var op = inputTree.Expr.FuncDecl;

            var flippable = new List<Z3_decl_kind>()
                    {
                        Z3_decl_kind.Z3_OP_LT,
                        Z3_decl_kind.Z3_OP_GT,
                        Z3_decl_kind.Z3_OP_LE,
                        Z3_decl_kind.Z3_OP_GE
                    };

            if (!(flippable.Contains(op.DeclKind) && op.Name.ToString() == symbol))
            {
                return inputTree;
            }

            if (!inputTree.HasIdentifier(name))
            {
                return inputTree;
            }

            Expr result = null;

            switch (op.DeclKind)
            {
                case Z3_decl_kind.Z3_OP_LT:
                    result = ctx.MkGt((ArithExpr)children[1].Expr, (ArithExpr)children[0].Expr);
                    break;
                case Z3_decl_kind.Z3_OP_GT:
                    result = ctx.MkLt((ArithExpr)children[1].Expr, (ArithExpr)children[0].Expr);
                    break;
                case Z3_decl_kind.Z3_OP_LE:
                    result = ctx.MkGe((ArithExpr)children[1].Expr, (ArithExpr)children[0].Expr);
                    break;
                case Z3_decl_kind.Z3_OP_GE:
                    result = ctx.MkLe((ArithExpr)children[1].Expr, (ArithExpr)children[0].Expr);
                    break;
            }

            return Utils.HandleSmtLibParsed(result, ctx);
        }
    }
}