using CmmInterpretor.Utils.Exceptions;
using CmmInterpretor.Expressions;
using CmmInterpretor.Extensions;
using CmmInterpretor.Operators.Bitwise;
using CmmInterpretor.Tokens;
using System.Collections.Generic;

namespace CmmInterpretor
{
    public static partial class ExpressionParser
    {
        private static IExpression ParseLogicalORs(List<Token> tokens, int precedence)
        {
            for (int i = tokens.Count - 1; i >= 0; i--)
            {
                if (tokens[i] is { type: TokenType.Operator, value: "|" })
                {
                    if (i == 0)
                        throw new SyntaxError("Missing the left part of logical OR");

                    if (i == tokens.Count - 1)
                        throw new SyntaxError("Missing the right part of logical OR");

                    var a = ParseLogicalORs(tokens.GetRange(..i), precedence);
                    var b = Parse(tokens.GetRange((i + 1)..), precedence - 1);

                    return new BitwiseOr(a, b);
                }
            }

            return Parse(tokens, precedence - 1);
        }
    }
}
