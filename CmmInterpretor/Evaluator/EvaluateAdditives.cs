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
                if (expr[i].type == TokenType.Operator)
                {
                    string op = expr[i].Text;

                    if (op is "+" or "-")
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

                        var a = EvaluateAdditives(expr.GetRange(..i), call, precedence);

                        if (a is not IValue aa)
                            return a;

                        var b = Evaluate(expr.GetRange((i + 1)..), call, precedence - 1);

                        if (b is not IValue bb)
                            return b;

                        if (op == "+")
                            return Operator.Add(aa.Value(), bb.Value());

                        if (op == "-")
                            return Operator.Substract(aa.Value(), bb.Value());
                    }
                }
            }

            return Evaluate(expr, call, precedence - 1);
        }
    }
}
