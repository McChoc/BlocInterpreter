using System.Collections.Generic;
using CmmInterpretor.Expressions;
using CmmInterpretor.Extensions;
using CmmInterpretor.Operators.Bitwise;
using CmmInterpretor.Tokens;
using CmmInterpretor.Utils.Exceptions;

namespace CmmInterpretor
{
    internal static partial class ExpressionParser
    {
        private static IExpression ParseLogicalANDs(List<Token> tokens, int precedence)
        {
            for (var i = tokens.Count - 1; i >= 0; i--)
            {
                if (tokens[i] is (TokenType.Operator, "&") op)
                {
                    if (i == 0)
                        throw new SyntaxError(op.Start, op.End, "Missing the left part of logical AND");

                    if (i == tokens.Count - 1)
                        throw new SyntaxError(op.Start, op.End, "Missing the right part of logical AND");

                    var a = ParseLogicalANDs(tokens.GetRange(..i), precedence);
                    var b = Parse(tokens.GetRange((i + 1)..), precedence - 1);

                    return new BitwiseAnd(a, b);
                }
            }

            return Parse(tokens, precedence - 1);
        }
    }
}