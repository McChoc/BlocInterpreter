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
        private static IExpression ParseCoalescings(List<Token> tokens, int precedence)
        {
            for (var i = tokens.Count - 1; i >= 0; i--)
            {
                if (tokens[i] is (TokenType.Operator, "??" or "???") @operator)
                {
                    if (i == 0)
                        throw new SyntaxError(@operator.Start, @operator.End, "Missing the left part of coalescing");

                    if (i == tokens.Count - 1)
                        throw new SyntaxError(@operator.Start, @operator.End, "Missing the right part of coalescing");

                    var left = ParseCoalescings(tokens.GetRange(..i), precedence);
                    var right = Parse(tokens.GetRange((i + 1)..), precedence);

                    return @operator.Text switch
                    {
                        "??" => new NullCoalescing(left, right),
                        "???" => new VoidCoalescing(left, right),
                        _ => throw new Exception()
                    };
                }
            }

            return Parse(tokens, precedence - 1);
        }
    }
}