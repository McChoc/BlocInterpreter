using System;
using System.Collections.Generic;
using Bloc.Exceptions;
using Bloc.Expressions;
using Bloc.Extensions;
using Bloc.Operators;
using Bloc.Tokens;

namespace Bloc
{
    internal static partial class ExpressionParser
    {
        private static IExpression ParseAdditives(List<Token> tokens, int precedence)
        {
            for (var i = tokens.Count - 1; i >= 0; i--)
            {
                if (tokens[i] is (TokenType.Operator, "+" or "-") @operator)
                {
                    if (i == 0)
                        continue;

                    if (i == 1 &&
                        tokens[0].Type is TokenType.Operator or TokenType.Keyword)
                        continue;

                    if (i >= 2 &&
                        tokens[i - 1].Type is TokenType.Operator or TokenType.Keyword &&
                        tokens[i - 2].Type != TokenType.Identifier)
                        continue;

                    if (i == tokens.Count - 1)
                        throw new SyntaxError(@operator.Start, @operator.End, "Missing right part of additive");

                    var left = ParseAdditives(tokens.GetRange(..i), precedence);
                    var right = Parse(tokens.GetRange((i + 1)..), precedence - 1);

                    return @operator.Text switch
                    {
                        "+" => new Addition(left, right),
                        "-" => new Substraction(left, right),
                        _ => throw new Exception()
                    };
                }
            }

            return Parse(tokens, precedence - 1);
        }
    }
}