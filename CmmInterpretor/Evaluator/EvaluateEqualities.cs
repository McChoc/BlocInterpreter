using CmmInterpretor.Data;
using CmmInterpretor.Exceptions;
using CmmInterpretor.Extensions;
using CmmInterpretor.Results;
using CmmInterpretor.Tokens;
using CmmInterpretor.Values;
using System.Collections.Generic;

namespace CmmInterpretor
{
    public static partial class Evaluator
    {
        private static IResult EvaluateEqualities(List<Token> expr, Call call, int precedence)
        {
            for (int i = expr.Count - 1; i >= 0; i--)
            {
                if (expr[i].type == TokenType.Operator)
                {
                    string op = expr[i].Text;

                    if (op is "==" or "!=")
                    {
                        if (i == 0)
                            throw new SyntaxError("Missing the left part of equality");

                        if (i > expr.Count - 1)
                            throw new SyntaxError("Missing the right part of equality");

                        var a = EvaluateEqualities(expr.GetRange(..i), call, precedence);

                        if (a is not IValue aa)
                            return a;

                        var b = Evaluate(expr.GetRange((i + 1)..), call, precedence - 1);

                        if (b is not IValue bb)
                            return b;

                        bool equality = aa.Value().Equals(bb.Value());

                        return new Bool(op == "==" ? equality : !equality);
                    }
                }
            }

            return Evaluate(expr, call, precedence - 1);
        }
    }
}
