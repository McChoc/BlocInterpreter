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
    private static IExpression ParseRanges(List<Token> tokens, int precedence)
    {
        var parts = tokens.Split(x => x is (TokenType.Symbol, Symbol.COLON));

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

        throw new SyntaxError(tokens[0].Start, tokens[^1].End, $"Unexpected symbol '{Symbol.COLON}'");
    }
}