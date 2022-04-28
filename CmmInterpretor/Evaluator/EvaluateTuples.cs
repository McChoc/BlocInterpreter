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
        private static IResult EvaluateTuples(List<Token> expr, Call call, int precedence)
        {
            var expressions = expr.Split(Token.Comma);

            if (expressions.Count == 0)
                throw new SyntaxError("Missing expression >:(");

            if (expressions.Count == 1)
                return Evaluate(expr, call, precedence - 1);
            
            var values = new List<IValue>();

            foreach (var expression in expressions)
            {
                var result = Evaluate(expression, call, precedence - 1);

                if (result is not IValue value)
                    return result;

                values.Add(value);
            }

            return new Tuple(values);
        }
    }
}
