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
                if (expr[i] is { type: TokenType.Operator, value: "==" or "!=" })
                {
                    if (i == 0)
                        throw new SyntaxError("Missing the left part of equality");

                    if (i > expr.Count - 1)
                        throw new SyntaxError("Missing the right part of equality");

                    var resultA = EvaluateEqualities(expr.GetRange(..i), call, precedence);

                    if (resultA is not IValue a)
                        return resultA;

                    var resultB = Evaluate(expr.GetRange((i + 1)..), call, precedence - 1);

                    if (resultB is not IValue b)
                        return resultB;

                    return expr[i].value switch
                    {
                        "==" => new Bool(a.Equals(b)),
                        "!=" => new Bool(!a.Equals(b)),
                        _ => throw new System.Exception()
                    };
                }
            }

            return Evaluate(expr, call, precedence - 1);
        }
    }
}
