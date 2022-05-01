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
        private static IResult EvaluateConditionalXORs(List<Token> expr, Call call, int precedence)
        {
            for (int i = expr.Count - 1; i >= 0; i--)
            {
                if (expr[i] is { type: TokenType.Operator, value: "^^" })
                {
                    if (i == 0)
                        throw new SyntaxError("Missing the left part of logical XOR");

                    if (i == expr.Count - 1)
                        throw new SyntaxError("Missing the right part of logical XOR");

                    var resultA = EvaluateConditionalXORs(expr.GetRange(..i), call, precedence);

                    if (resultA is not IValue a)
                        return resultA;

                    if (!a.Value.Implicit(out Bool boolA))
                        return new Throw("Cannot implicitly convert to bool");

                    var resultB = Evaluate(expr.GetRange((i + 1)..), call, precedence - 1);

                    if (resultB is not IValue b)
                        return resultB;

                    if (!b.Value.Implicit(out Bool boolB))
                        return new Throw("Cannot implicitly convert to bool");

                    return new Bool(boolA.Value != boolB.Value);
                }
            }

            return Evaluate(expr, call, precedence - 1);
        }
    }
}
