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
                if (expr[i].type == TokenType.Operator && expr[i].Text == "^^")
                {
                    if (i == 0)
                        throw new SyntaxError("Missing the left part of logical XOR");

                    if (i == expr.Count - 1)
                        throw new SyntaxError("Missing the right part of logical XOR");

                    var a = EvaluateConditionalXORs(expr.GetRange(..i), call, precedence);

                    if (a is not IValue aa)
                        return a;

                    if (!aa.Value().Implicit(out Bool aaa))
                        return new Throw("Cannot implicitly convert to bool");

                    var b = Evaluate(expr.GetRange((i + 1)..), call, precedence - 1);

                    if (b is not IValue bb)
                        return b;

                    if (!bb.Value().Implicit(out Bool bbb))
                        return new Throw("Cannot implicitly convert to bool");

                    return new Bool(aaa.Value != bbb.Value);
                }
            }

            return Evaluate(expr, call, precedence - 1);
        }
    }
}
