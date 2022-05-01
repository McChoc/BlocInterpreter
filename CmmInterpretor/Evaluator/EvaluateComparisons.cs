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
        private static IResult EvaluateComparisons(List<Token> expr, Call call, int precedence)
        {
            for (int i = expr.Count - 1; i >= 0; i--)
            {
                if (expr[i] is { type: TokenType.Operator, value: "<=>" })
                {
                    if (i == 0)
                        throw new SyntaxError("Missing the left part of comparison");

                    if (i > expr.Count - 1)
                        throw new SyntaxError("Missing the right part of comparison");

                    var resultA = EvaluateAdditives(expr.GetRange(..i), call, precedence);

                    if (resultA is not IValue a)
                        return resultA;

                    var resultB = Evaluate(expr.GetRange((i + 1)..), call, precedence - 1);

                    if (resultB is not IValue b)
                        return resultB;

                    return Operator.Compare(a, b);
                }
            }

            return Evaluate(expr, call, precedence - 1);
        }
    }
}
