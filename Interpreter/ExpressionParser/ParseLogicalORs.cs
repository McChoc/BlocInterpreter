using System.Collections.Generic;
using Bloc.Exceptions;
using Bloc.Expressions;
using Bloc.Extensions;
using Bloc.Operators;
using Bloc.Tokens;
using Bloc.Utils;

namespace Bloc;

internal static partial class ExpressionParser
{
    private static IExpression ParseLogicalORs(List<Token> tokens, int precedence)
    {
        for (var i = tokens.Count - 1; i >= 0; i--)
        {
            if (tokens[i] is (TokenType.Symbol, Symbol.BIT_OR) @operator)
            {
                if (i == 0)
                    throw new SyntaxError(@operator.Start, @operator.End, "Missing the left part of logical OR");

                if (i == tokens.Count - 1)
                    throw new SyntaxError(@operator.Start, @operator.End, "Missing the right part of logical OR");

                var left = ParseLogicalORs(tokens.GetRange(..i), precedence);
                var right = Parse(tokens.GetRange((i + 1)..), precedence - 1);

                return new BitwiseOr(left, right);
            }
        }

        return Parse(tokens, precedence - 1);
    }
}