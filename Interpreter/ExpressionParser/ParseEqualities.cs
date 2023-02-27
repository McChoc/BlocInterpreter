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
    private static IExpression ParseEqualities(List<Token> tokens, int precedence)
    {
        for (var i = tokens.Count - 1; i >= 0; i--)
        {
            if (IsEquality(tokens[i]))
            {
                var @operator = tokens[i];

                if (i == 0)
                    throw new SyntaxError(@operator.Start, @operator.End, "Missing the left part of equality");

                if (i > tokens.Count - 1)
                    throw new SyntaxError(@operator.Start, @operator.End, "Missing the right part of equality");

                var left = ParseEqualities(tokens.GetRange(..i), precedence);
                var right = Parse(tokens.GetRange((i + 1)..), precedence - 1);

                return @operator.Text == Symbol.IS_EQUAL
                    ? new Equality(left, right)
                    : new Inequality(left, right);
            }
        }

        return Parse(tokens, precedence - 1);
    }

    private static bool IsEquality(Token token)
    {
        return token is (TokenType.Symbol,
            Symbol.IS_EQUAL or
            Symbol.NOT_EQUAL_0 or
            Symbol.NOT_EQUAL_1 or
            Symbol.NOT_EQUAL_2);
    }
}