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
        private static IResult EvaluateTernaries(List<Token> expr, Call call, int precedence)
        {
            int idx1 = -1, idx2 = -1;

            int i = 0;

            for (; i <= expr.Count; i++)
            {
                if (i == expr.Count)
                    return Evaluate(expr, call, precedence - 1);

                if (expr[i] is { type: TokenType.Operator, value: "?" })
                {
                    idx1 = i;
                    break;
                }
            }

            int depth = 0;

            for (; i <= expr.Count; i++)
            {
                if (i == expr.Count)
                    throw new SyntaxError("Missing ':'.");

                if (expr[i] is { type: TokenType.Operator, value: "?" })
                    depth++;

                if (expr[i] is { type: TokenType.Operator, value: ":" })
                {
                    depth--;

                    if (depth == 0)
                    {
                        idx2 = i;
                        break;
                    }
                }
            }

            var result = Evaluate(expr.GetRange(..idx1), call, precedence - 1);

            if (result is not IValue value)
                return result;

            if (value.Implicit(out Bool b))
            {
                if (b.Value)
                    return EvaluateTernaries(expr.GetRange((idx1 + 1)..idx2), call, precedence);
                else
                    return EvaluateTernaries(expr.GetRange((idx2 + 1)..), call, precedence);
            }

            return new Throw("Cannot convert to bool");
        }
    }
}
