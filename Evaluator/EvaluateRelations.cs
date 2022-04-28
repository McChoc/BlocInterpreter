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
                if (expr[i].type is TokenType.Operator or TokenType.Keyword)
                {
                    string op = expr[i].Text;

                    if (op is "<" or "<=" or ">" or ">=" or "in" or "is" or "as")
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

                        return op switch
                        {
                            "<" => Operator.Less(aa.Value(), bb.Value()),
                            "<=" => Operator.LessOrEqual(aa.Value(), bb.Value()),
                            ">" => Operator.Greater(aa.Value(), bb.Value()),
                            ">=" => Operator.GreaterOrEqual(aa.Value(), bb.Value()),
                            "in" => Operator.In(aa.Value(), bb.Value()),
                            "is" => Operator.Is(aa.Value(), bb.Value()),
                            "as" => Operator.As(aa.Value(), bb.Value()),
                            _ => throw new System.Exception()
                        };
                    }
                }
            }

            return Evaluate(expr, call, precedence - 1);
        }
    }
}
