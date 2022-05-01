using CmmInterpretor.Data;
using CmmInterpretor.Exceptions;
using CmmInterpretor.Extensions;
using CmmInterpretor.Results;
using CmmInterpretor.Tokens;
using System.Collections.Generic;

namespace CmmInterpretor
{
    public static partial class Evaluator
    {
        private static IResult EvaluateRelations(List<Token> expr, Call call, int precedence)
        {
            for (int i = expr.Count - 1; i >= 0; i--)
            {
                if (expr[i] is { type: TokenType.Operator or TokenType.Keyword, value: "<" or "<=" or ">" or ">=" or "in" or "is" or "as" })
                {
                    if (i == 0)
                        throw new SyntaxError("Missing the left part of relation");

                    if (i > expr.Count - 1)
                        throw new SyntaxError("Missing the right part of relation");

                    var a = EvaluateRelations(expr.GetRange(..i), call, precedence);

                    if (a is not IValue aa)
                        return a;

                    var b = Evaluate(expr.GetRange((i + 1)..), call, precedence - 1);

                    if (b is not IValue bb)
                        return a;

                    return expr[i].value switch
                    {
                        "<" => Operator.Less(aa, bb),
                        "<=" => Operator.LessOrEqual(aa, bb),
                        ">" => Operator.Greater(aa, bb),
                        ">=" => Operator.GreaterOrEqual(aa, bb),
                        "in" => Operator.In(aa, bb),
                        "is" => Operator.Is(aa, bb),
                        "as" => Operator.As(aa, bb),
                        _ => throw new System.Exception()
                    };
                }
            }

            return Evaluate(expr, call, precedence - 1);
        }
    }
}
