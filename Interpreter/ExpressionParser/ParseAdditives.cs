using System;
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
    private static IExpression ParseAdditives(List<Token> tokens, int precedence)
    {
        for (var i = tokens.Count - 1; i >= 0; i--)
        {
            if (IsAdditive(tokens[i]))
            {
                var @operator = tokens[i];

                if (i == 0)
                    continue;

                if (i == 1 &&
                    tokens[0].Type is TokenType.Symbol or TokenType.Keyword)
                    continue;

                if (i >= 2 &&
                    tokens[i - 1].Type is TokenType.Symbol or TokenType.Keyword &&
                    tokens[i - 2].Type != TokenType.Identifier)
                    continue;

                if (i == tokens.Count - 1)
                    throw new SyntaxError(@operator.Start, @operator.End, "Missing right part of additive");

                var left = ParseAdditives(tokens.GetRange(..i), precedence);
                var right = Parse(tokens.GetRange((i + 1)..), precedence - 1);

                return @operator.Text switch
                {
                    Symbol.PLUS => new Addition(left, right),
                    Symbol.MINUS => new Substraction(left, right),
                    _ => throw new Exception()
                };
            }
        }

        return Parse(tokens, precedence - 1);
    }

    private static bool IsAdditive(Token token)
    {
        return token is (TokenType.Symbol,
            Symbol.PLUS or
            Symbol.MINUS);
    }
}