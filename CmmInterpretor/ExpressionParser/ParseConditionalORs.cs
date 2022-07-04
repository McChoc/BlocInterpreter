using System.Collections.Generic;
using CmmInterpretor.Expressions;
using CmmInterpretor.Extensions;
using CmmInterpretor.Operators.Boolean;
using CmmInterpretor.Tokens;
using CmmInterpretor.Utils.Exceptions;

namespace CmmInterpretor
{
    internal static partial class ExpressionParser
    {
        private static IExpression ParseConditionalORs(List<Token> tokens, int precedence)
        {
            for (var i = tokens.Count - 1; i >= 0; i--)
            {
                if (tokens[i] is (TokenType.Operator, "||") op)
                {
                    if (i == 0)
                        throw new SyntaxError(op.Start, op.End, "Missing the left part of logical OR");

                    if (i == tokens.Count - 1)
                        throw new SyntaxError(op.Start, op.End, "Missing the right part of logical OR");

                    var a = ParseConditionalORs(tokens.GetRange(..i), precedence);
                    var b = Parse(tokens.GetRange((i + 1)..), precedence);

                    return new BooleanOr(a, b);
                }
            }

            return Parse(tokens, precedence - 1);
        }
    }
}