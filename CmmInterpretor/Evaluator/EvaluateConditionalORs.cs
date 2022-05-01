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
        private static IResult EvaluateConditionalORs(List<Token> expr, Call call, int precedence)
        {
            for (int i = expr.Count - 1; i >= 0; i--)
            {
                if (expr[i] is { type: TokenType.Operator, value: "||" })
                {
                    if (i == 0)
                        throw new SyntaxError("Missing the left part of logical OR");

                    if (i == expr.Count - 1)
                        throw new SyntaxError("Missing the right part of logical OR");

                    {
                        var result = EvaluateConditionalORs(expr.GetRange(..i), call, precedence);

                        if (result is not IValue value)
                            return result;

                        if (!value.Implicit(out Bool @bool))
                            return new Throw("Cannot implicitly convert to bool");

                        if (@bool.Value)
                            return value.Value;
                    }

                    {
                        var result = Evaluate(expr.GetRange((i + 1)..), call, precedence - 1);

                        if (result is not IValue value)
                            return result;

                        if (!value.Implicit(out Bool _))
                            return new Throw("Cannot implicitly convert to bool");

                        return value.Value;
                    }
                }
            }

            return Evaluate(expr, call, precedence - 1);
        }
    }
}
