using CmmInterpretor.Utils.Exceptions;
using CmmInterpretor.Expressions;
using CmmInterpretor.Extensions;
using CmmInterpretor.Tokens;
using System.Collections.Generic;

namespace CmmInterpretor
{
    public static partial class ExpressionParser
    {
        private static IExpression ParseTuples(List<Token> tokens, int precedence)
        {
            var parts = tokens.Split(Token.Comma);

            if (parts.Count == 0)
                throw new SyntaxError("Missing expression");

            if (parts.Count == 1)
                return Parse(tokens, precedence - 1);
            
            var expressions = new List<IExpression>();

            foreach (var part in parts)
                expressions.Add(Parse(part, precedence - 1));

            return new TupleLiteral(expressions);
        }
    }
}
