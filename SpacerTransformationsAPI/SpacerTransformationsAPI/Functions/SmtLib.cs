using Microsoft.Z3;

namespace SpacerTransformationsAPI.Functions
{
    public static class SmtLib
    {
        
        public static Expr StringToSmtLib(Context ctx, string raw)
        {
            var fmls = ctx.ParseSMTLIB2String(raw);
            return fmls[0];
        }
        /*

        public static void ParserExample1(Context ctx)
        {
            Console.WriteLine("ParserExample1");

            var fmls = ctx.ParseSMTLIB2String("(declare-const x Int) (declare-const y Int) (assert (> x y)) (assert (> x 0))");

            Console.WriteLine("formula: {0}", fmls[0]);
        }
        public static void ParserExample2(Context ctx)
        {
           Console.WriteLine("ParserExample2");
           Symbol[] declNames = { ctx.MkSymbol("a"), ctx.MkSymbol("b") };

           var a = ctx.MkConstDecl(declNames[0], ctx.MkIntSort());
           var b = ctx.MkConstDecl(declNames[1], ctx.MkIntSort());
           var decls = new[] { a, b };
           var f = ctx.ParseSMTLIB2String("(assert (> a b))", null, null, declNames, decls)[0];

           Console.WriteLine("formula: {0}", f);
        }
        private const string PathToFiles = @"./../../../smtlib/";
        public static Expr Smt2FileTest(Context ctx, string filename)
        {
            GC.Collect();

            var fmls = ctx.ParseSMTLIB2File(PathToFiles + filename);

            // Iterate over the formula.
            if (fmls.Length == 0) return null;
            var q = new Queue();
            q.Enqueue(fmls[0]);
            while (q.Count > 0)
            {
                var cur = (AST) q.Dequeue();

                if (!(cur is Expr expr) || expr.IsVar) continue;
                foreach (var c in expr.Args)
                {
                    q.Enqueue(c);
                }
            }
            return fmls[0];

        }
        */
    }
}