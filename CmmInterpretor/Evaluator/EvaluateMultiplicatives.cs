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
        private static IResult EvaluateMultiplicatives(List<Token> expr, Call call, int precedence)
        {
            for (int i = expr.Count - 1; i >= 0; i--)
            {
                if (expr[i].type == TokenType.Operator)
                {
                    string op = expr[i].Text;

                    if (op is "*" or "/" or "%")
                    {
                        if (i == 0)
                            throw new SyntaxError("Missing the left part of multiplicative");

                        if (i > expr.Count - 1)
                            throw new SyntaxError("Missing the right part of multiplicative");

                        var a = EvaluateMultiplicatives(expr.GetRange(..i), call, precedence);

                        if (a is not IValue aa)
                            return a;

                        var b = Evaluate(expr.GetRange((i + 1)..), call, precedence - 1);

                        if (b is not IValue bb)
                            return b;

                        return op switch
                        {
                            "*" => Operator.Multiply(aa.Value(), bb.Value()),
                            "/" => Operator.Divide(aa.Value(), bb.Value()),
                            "%" => Operator.Remainder(aa.Value(), bb.Value()),
                            _ => throw new System.Exception()
                        };
                    }
                }
            }

            return Evaluate(expr, call, precedence - 1);
        }
    }
}
