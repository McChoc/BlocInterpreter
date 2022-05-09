using CmmInterpretor.Data;
using CmmInterpretor.Extensions;
using CmmInterpretor.Results;
using CmmInterpretor.Tokens;
using CmmInterpretor.Values;
using System.Collections.Generic;

namespace CmmInterpretor
{
    public static partial class Evaluator
    {
        private static IResult EvaluateTernaries(List<Token> expr, Call call, int precedence)
        {
            for (int i = 0; i < expr.Count; i++)
            {
                if (expr[i] is { type: TokenType.Operator, value: "?" })
                {
                    int j = i, depth = 0;

                    for (; j < expr.Count; j++)
                    {
                        if (expr[j] is { type: TokenType.Operator, value: "?" })
                            depth++;

                        if (expr[j] is { type: TokenType.Operator, value: ":" })
                        {
                            depth--;

                            if (depth == 0)
                            {
                                var result = Evaluate(expr.GetRange(..i), call, precedence - 1);

                                if (result is not IValue value)
                                    return result;

                                if (!value.Implicit(out Bool b))
                                    return new Throw("Cannot convert to bool");

                                if (b.Value)
                                    return EvaluateTernaries(expr.GetRange((i + 1)..j), call, precedence);
                                else
                                    return EvaluateTernaries(expr.GetRange((j + 1)..), call, precedence);
                            }
                        }
                    }
                }
            }

            return Evaluate(expr, call, precedence - 1);
        }
    }
}
