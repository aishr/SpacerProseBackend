using System.Collections.Generic;
using Microsoft.Z3;

namespace SpacerTransformationsAPI.Functions
{
    public static class ReadableParser
    {
        //Assume lst is array of strings and sep is string
        public static string ParseResult(Expr expr, bool addFalse = false)
        {
            var result = "";
            //Replaces lines 16-19 of readable.ts
            //symbols for logical relations
            var logSym = new Dictionary<Z3_decl_kind, string>()
            {
                {Z3_decl_kind.Z3_OP_AND, "&&"}, 
                {Z3_decl_kind.Z3_OP_OR, "||"}
            };

            if (expr.Args.Length == 0)
            {
                var x = expr.ToString();
                return x;
                //parseResult returns string
            }

            //logical symbol should be inserted between each child clause
            //logSym.ContainsKey(lst[0]) should do same thing as lst[0] in logSym in line 31 of readable.ts
            if (logSym.ContainsKey(expr.FuncDecl.DeclKind))
            {
                //Not sure what array.splice() is in TypeScript with one argument
                //Ash: same as js! one argument just means take the entire rest of the list starting at the argument index given
                //Ash: Given an array x = [1, 2, 3, 4], splice(1) -> [2, 3, 4]
                for (var i = 0; i < expr.Args.Length; i++)
                {
                    if (i == expr.Args.Length - 1)
                    {
                        result += ParseResult(expr.Args[i], addFalse) + (addFalse ? " =>\nfalse" : "");
                        return result;
                    }
                    result += ParseResult(expr.Args[i], addFalse) + " " + logSym[expr.FuncDecl.DeclKind] + "\n";
                }
            }

            if (expr.FuncDecl.DeclKind == Z3_decl_kind.Z3_OP_IMPLIES)
            {
                return ParseResult(expr.Args[0], addFalse) + " " + expr.FuncDecl.Name + "\n" + ParseResult(expr.Args[1], addFalse);
            }
            //handles indexing into an array
            if (expr.FuncDecl.DeclKind == Z3_decl_kind.Z3_OP_SELECT)
            {
                //parseResult expects string[] as first argument
                return ParseResult(expr.Args[0], addFalse) + "[" + ParseResult(expr.Args[1], addFalse) + "]";
            }

            if (expr.FuncDecl.DeclKind == Z3_decl_kind.Z3_OP_UMINUS)
            {
                return "(-" + ParseResult(expr.Args[0], addFalse) + ")";
            }

            //Adds not symbol (!) to beginning of clause
            if (expr.FuncDecl.DeclKind == Z3_decl_kind.Z3_OP_NOT)
            {
                //parseResult expects string[] as first argument
                return "!(" + ParseResult(expr.Args[0], addFalse) + ")";
            }
            return "(" + ParseResult(expr.Args[0], addFalse) + " " + expr.FuncDecl.Name + " " + ParseResult(expr.Args[1], addFalse) + ")";
        }

        /*
        private static string ReplaceVarNames(string expr, string[] varList)
        {
            for(var i = 0; i < varList.Length; ++i)
            {
                //Meant to replace lines 78-79 in readable.ts
                //Assuming "gi" means case insensitivity
                expr = Regex.Replace(expr, @"Inv_" + i + "_n", varList[i], RegexOptions.IgnoreCase);
            }
               
            return expr;
        }
        */
    }
}