﻿using System.Collections.Generic;
using Bloc.Exceptions;
using Bloc.Expressions;
using Bloc.Extensions;
using Bloc.Operators;
using Bloc.Tokens;

namespace Bloc
{
    internal static partial class ExpressionParser
    {
        private static IExpression ParseCatchExpressions(List<Token> tokens, int precedence)
        {
            for (var i = tokens.Count - 1; i >= 0; i--)
            {
                if (tokens[i] is (TokenType.Keyword, "catch") @operator)
                {
                    if (i == 0)
                        throw new SyntaxError(@operator.Start, @operator.End, "Missing the left part of catch expression");

                    if (i == tokens.Count - 1)
                        throw new SyntaxError(@operator.Start, @operator.End, "Missing the right part of catch expression");

                    var left = ParseCoalescings(tokens.GetRange(..i), precedence);
                    var right = Parse(tokens.GetRange((i + 1)..), precedence);

                    return new CatchExpression(left, right);
                }
            }

            return Parse(tokens, precedence - 1);
        }
    }
}