using System;
using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Extensions;
using Bloc.Operators;
using Bloc.Tokens;
using Bloc.Utils.Exceptions;

namespace Bloc
{
    internal static partial class ExpressionParser
    {
        private static IExpression ParseExponentials(List<Token> tokens, int precedence)
        {
            for (var i = tokens.Count - 1; i >= 0; i--)
            {
                if (tokens[i] is (TokenType.Operator, "**" or "//" or "%%") @operator)
                {
                    if (i == 0)
                        throw new SyntaxError(@operator.Start, @operator.End, "Missing the left part of exponential");

                    if (i > tokens.Count - 1)
                        throw new SyntaxError(@operator.Start, @operator.End, "Missing the right part of exponential");

                    var left = ParseExponentials(tokens.GetRange(..i), precedence);
                    var right = Parse(tokens.GetRange((i + 1)..), precedence - 1);

                    return @operator.Text switch
                    {
                        "**" => new Power(left, right),
                        "//" => new Root(left, right),
                        "%%" => new Logarithm(left, right),
                        _ => throw new Exception()
                    };
                }
            }

            return Parse(tokens, precedence - 1);
        }
    }
}