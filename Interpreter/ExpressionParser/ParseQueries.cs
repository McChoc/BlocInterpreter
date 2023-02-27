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
    private static IExpression ParseQueries(List<Token> tokens, int precedence)
    {
        for (var i = tokens.Count - 1; i >= 0; i--)
        {
            if (IsQuery(tokens[i]))
            {
                var @operator = tokens[i];

                if (i == 0)
                    throw new SyntaxError(@operator.Start, @operator.End, "Missing the left part of query");

                if (i == tokens.Count - 1)
                    throw new SyntaxError(@operator.Start, @operator.End, "Missing the right part of query");

                var left = ParseQueries(tokens.GetRange(..i), precedence);
                var right = Parse(tokens.GetRange((i + 1)..), precedence - 1);

                return @operator.Text switch
                {
                    Keyword.SELECT  => new Select(left, right),
                    Keyword.WHERE   => new Where(left, right),
                    Keyword.ORDERBY => new Orderby(left, right),
                    _ => throw new Exception()
                };
            }
        }

        return Parse(tokens, precedence - 1);
    }

    private static bool IsQuery(Token token)
    {
        return token is (TokenType.Keyword,
            Keyword.SELECT or
            Keyword.WHERE or
            Keyword.ORDERBY);
    }
}