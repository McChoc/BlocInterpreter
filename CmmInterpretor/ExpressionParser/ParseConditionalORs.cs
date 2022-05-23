using CmmInterpretor.Utils.Exceptions;
using CmmInterpretor.Expressions;
using CmmInterpretor.Extensions;
using CmmInterpretor.Operators.Boolean;
using CmmInterpretor.Tokens;
using System.Collections.Generic;

namespace CmmInterpretor
{
    public static partial class ExpressionParser
    {
        private static IExpression ParseConditionalORs(List<Token> tokens, int precedence)
        {
            for (int i = tokens.Count - 1; i >= 0; i--)
            {
                if (tokens[i] is { type: TokenType.Operator, value: "||" })
                {
                    if (i == 0)
                        throw new SyntaxError("Missing the left part of logical OR");

                    if (i == tokens.Count - 1)
                        throw new SyntaxError("Missing the right part of logical OR");

                    var a = ParseConditionalORs(tokens.GetRange(..i), precedence);
                    var b = Parse(tokens.GetRange((i + 1)..), precedence);

                    return new BooleanOr(a, b);
                }
            }

            return Parse(tokens, precedence - 1);
        }
    }
}
