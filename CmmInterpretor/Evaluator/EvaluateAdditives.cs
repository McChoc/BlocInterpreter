using CmmInterpretor.Data;
using CmmInterpretor.Extensions;
using CmmInterpretor.Results;
using CmmInterpretor.Tokens;
using System.Collections.Generic;

namespace CmmInterpretor
{
    public static partial class Evaluator
    {
        private static IResult EvaluateAdditives(List<Token> expr, Call call, int precedence)
        {
            for (int i = expr.Count - 1; i >= 0; i--)
            {
                if (expr[i] is { type: TokenType.Operator, value: "+" or "-" })
                {
                    if (i == expr.Count - 1)
                        return new Throw("Missing right part of additive");

                    if (i == 0)
                        continue;

                    if (i == 1 &&
                        expr[0].type is TokenType.Operator or TokenType.Keyword)
                        continue;

                    if (i >= 2 &&
                        expr[i - 1].type is TokenType.Operator or TokenType.Keyword &&
                        expr[i - 2].type is not TokenType.Identifier)
                        continue;

                    var resultA = EvaluateAdditives(expr.GetRange(..i), call, precedence);

                    if (resultA is not IValue a)
                        return resultA;

                    var resultB = Evaluate(expr.GetRange((i + 1)..), call, precedence - 1);

                    if (resultB is not IValue b)
                        return resultB;

                    return expr[i].value switch
                    {
                        "+" => Operator.Add(a, b),
                        "-" => Operator.Substract(a, b),
                        _ => throw new System.Exception()
                    };
                }
            }

            return Evaluate(expr, call, precedence - 1);
        }
    }
}
