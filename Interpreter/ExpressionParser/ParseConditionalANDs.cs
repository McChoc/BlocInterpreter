using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Extensions;
using Bloc.Operators.Boolean;
using Bloc.Tokens;
using Bloc.Utils.Exceptions;

namespace Bloc
{
    internal static partial class ExpressionParser
    {
        private static IExpression ParseConditionalANDs(List<Token> tokens, int precedence)
        {
            for (var i = tokens.Count - 1; i >= 0; i--)
            {
                if (tokens[i] is (TokenType.Operator, "&&") op)
                {
                    if (i == 0)
                        throw new SyntaxError(op.Start, op.End, "Missing the left part of logical AND");

                    if (i == tokens.Count - 1)
                        throw new SyntaxError(op.Start, op.End, "Missing the right part of logical AND");

                    var a = ParseConditionalANDs(tokens.GetRange(..i), precedence);
                    var b = Parse(tokens.GetRange((i + 1)..), precedence);

                    return new BooleanAnd(a, b);
                }
            }

            return Parse(tokens, precedence - 1);
        }
    }
}