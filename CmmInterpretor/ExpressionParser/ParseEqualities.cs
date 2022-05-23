using CmmInterpretor.Utils.Exceptions;
using CmmInterpretor.Expressions;
using CmmInterpretor.Extensions;
using CmmInterpretor.Operators.Relation;
using CmmInterpretor.Tokens;
using System.Collections.Generic;

namespace CmmInterpretor
{
    public static partial class ExpressionParser
    {
        private static IExpression ParseEqualities(List<Token> tokens, int precedence)
        {
            for (int i = tokens.Count - 1; i >= 0; i--)
            {
                if (tokens[i] is { type: TokenType.Operator, value: "==" or "!=" or "<>" })
                {
                    if (i == 0)
                        throw new SyntaxError("Missing the left part of equality");

                    if (i > tokens.Count - 1)
                        throw new SyntaxError("Missing the right part of equality");

                    var a = ParseEqualities(tokens.GetRange(..i), precedence);
                    var b = Parse(tokens.GetRange((i + 1)..), precedence - 1);

                    return tokens[i].value switch
                    {
                        "==" => new Equality(a, b),
                        _ => new Inequality(a, b)
                    };
                }
            }

            return Parse(tokens, precedence - 1);
        }
    }
}
