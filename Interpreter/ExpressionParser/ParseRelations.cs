using System;
using System.Collections.Generic;
using Bloc.Constants;
using Bloc.Exceptions;
using Bloc.Expressions;
using Bloc.Operators;
using Bloc.Tokens;
using Bloc.Utils.Extensions;

namespace Bloc;

internal static partial class ExpressionParser
{
    private static IExpression ParseRelations(List<Token> tokens, int precedence)
    {
        for (var i = tokens.Count - 1; i >= 0; i--)
        {
            if (IsRelation(tokens[i], out var @operator))
            {
                if (i == 0)
                    throw new SyntaxError(@operator!.Start, @operator.End, "Missing the left part of relation");

                if (i > tokens.Count - 1)
                    throw new SyntaxError(@operator!.Start, @operator.End, "Missing the right part of relation");

                var left = ParseRelations(tokens.GetRange(..i), precedence);
                var right = Parse(tokens.GetRange((i + 1)..), precedence - 1);

                return @operator!.Text switch
                {
                    Symbol.LESS_THAN    => new Less(left, right),
                    Symbol.LESS_EQUAL   => new LessEqual(left, right),
                    Symbol.MORE_THAN    => new Greater(left, right),
                    Symbol.MORE_EQUAL   => new GreaterEqual(left, right),
                    Keyword.IN          => new In(left, right),
                    Keyword.NOT_IN      => new NotIn(left, right),
                    Keyword.IS          => new Is(left, right),
                    Keyword.IS_NOT      => new IsNot(left, right),
                    Keyword.AS          => new As(left, right),
                    _ => throw new Exception()
                };
            }
        }

        return Parse(tokens, precedence - 1);
    }

    private static bool IsRelation(Token token, out TextToken? @operator)
    {
        if (token is
            SymbolToken(Symbol.LESS_THAN or Symbol.LESS_EQUAL or Symbol.MORE_THAN or Symbol.MORE_EQUAL) or
            KeywordToken(Keyword.IN or Keyword.NOT_IN or Keyword.IS or Keyword.IS_NOT or Keyword.AS))
        {
            @operator = (TextToken)token;
            return true;
        }
        else
        {
            @operator = null;
            return false;
        }
    }
}