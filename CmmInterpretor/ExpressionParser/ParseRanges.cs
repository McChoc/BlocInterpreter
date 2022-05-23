using CmmInterpretor.Utils.Exceptions;
using CmmInterpretor.Expressions;
using CmmInterpretor.Extensions;
using CmmInterpretor.Operators.Arithmetic;
using CmmInterpretor.Tokens;
using System.Collections.Generic;

namespace CmmInterpretor
{
    public static partial class ExpressionParser
    {
        private static IExpression ParseRanges(List<Token> tokens, int precedence)
        {
            var parts = tokens.Split(new Token(TokenType.Operator, ".."));

            if (parts.Count == 0)
                throw new SyntaxError("Missing expression");

            if (parts.Count == 1)
                return Parse(parts[0], precedence - 1);

            if (parts.Count == 2)
            {
                var start = parts[0].Count > 0 ? Parse(parts[0], precedence - 1) : null;
                var end = parts[1].Count > 0 ? Parse(parts[1], precedence - 1) : null;

                return new Range(start, end, null);
            }

            if (parts.Count == 3)
            {
                var start = parts[0].Count > 0 ? Parse(parts[0], precedence - 1) : null;
                var end = parts[1].Count > 0 ? Parse(parts[1], precedence - 1) : null;
                var step = parts[2].Count > 0 ? Parse(parts[2], precedence - 1) : null;

                return new Range(start, end, step);
            }

            throw new SyntaxError("Unexpected symbol '..'");
        }
    }
}
