using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Z3;
using SpacerTransformationsAPI.Functions;

namespace SpacerTransformationsAPI.Models
{
    public class Node
    {
        private static int _idCounter;
        public Node(Z3_decl_kind type, List<Node> children, Expr expr, Context ctx)
        {
            _id = _idCounter;
            _idCounter++;
            Expr = expr;
            Children = children;
            Type = type;
            Ctx = ctx;
        }

        public List<Node> Children { get; }

        private Node Parent { get; set; }
        private int _id;
        public Expr Expr { get; }
        public Context Ctx { get; }
        public Z3_decl_kind Type { get; }

        public void AddChild(Node child, int position = -1)
        {
            if (child.Parent == null)
            {
                child.Parent = this;
            }
            if (position != -1)
            {
                Children.Insert(position, child);
            }
            else
            {
                Children.Add(child);
            }
        }

        public override string ToString()
        {
            return Expr.ToString();
        }

        public bool IsEqualTo(Node tree)
        {
            if (tree is null)
            {
                return false;
            }
            if (Children?.Count == 0 && tree.Children.Count == 0)
            {
                return Expr.ToString() == tree.Expr.ToString();
            }
            if (Children?.Count != tree.Children.Count)
            {
                return false;
            }
            var result = true;
            for (var i = 0; i < Children.Count; i++)
            {
                result = result && Children[i].Equals(tree.Children[i]);
            }

            return Type == tree.Type && result;
        }
        
        public override bool Equals(object obj)
        {
            return IsEqualTo(obj as Node);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int) Type);
        }

        public IEnumerable<string> GetIdentifiers()
        {
            var result = new List<string>();
            if (Children.Count == 0)
            {
                if (Expr.FuncDecl.DeclKind == Z3_decl_kind.Z3_OP_UNINTERPRETED)
                {
                    result.Add(Expr.ToString());
                }
            }
            foreach (var child in Children)
            {
                result = result.Concat(child.GetIdentifiers()).ToList();
            }
            return result;
        }

        public bool HasIdentifier(string id)
        {
            var result = false;
            if (Children.Count != 0)
            {
                foreach (var child in Children)
                {
                    result = result || child.HasIdentifier(id);
                }
            }
            return result || Expr.ToString() == id;
        }

        public bool IsNot()
        {
            return Expr.FuncDecl.DeclKind == Z3_decl_kind.Z3_OP_NOT;
        }
        
        public IList<Expr> FlattenTree()
        {
            var exprs = new List<Expr>();
            if (Expr.NumArgs == 0 || Expr.FuncDecl.DeclKind == Z3_decl_kind.Z3_OP_SELECT)
            {
                exprs.Add(Expr);
            }
            foreach (var child in Children)
            {
                exprs = exprs.Concat(child.FlattenTree()).ToList();
            }

            return exprs;
        }
        
        
        public IEnumerable<string> GetProcesses()
        {
            var children = new List<string>();

            foreach (var child in Children)
            {
                var exprs = child.FlattenTree();
                var indices = Utils.FindSelect(exprs.ToList());

                foreach (int index in indices)
                {
                    var selectExpr = exprs[index];
                    var process = selectExpr.Args[1].ToString();

                    if (!children.Contains(process))
                    {
                        children.Add(process);
                    }
                }
            }

            return children;
        }
    }
}