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
    private static IExpression ParseShifts(List<Token> tokens, int precedence)
    {
        for (var i = tokens.Count - 1; i >= 0; i--)
        {
            if (IsShift(tokens[i]))
            {
                var @operator = tokens[i];

                if (i == 0)
                    throw new SyntaxError(@operator.Start, @operator.End, "Missing the left part of shift");

                if (i > tokens.Count - 1)
                    throw new SyntaxError(@operator.Start, @operator.End, "Missing the right part of shift");

                var left = ParseShifts(tokens.GetRange(..i), precedence);
                var right = Parse(tokens.GetRange((i + 1)..), precedence - 1);

                return @operator.Text switch
                {
                    Symbol.SHIFT_L => new LeftShift(left, right),
                    Symbol.SHIFT_R => new RightShift(left, right),
                    _ => throw new Exception()
                };
            }
        }

        return Parse(tokens, precedence - 1);
    }

    private static bool IsShift(Token token)
    {
        return token is (TokenType.Symbol,
            Symbol.SHIFT_L or
            Symbol.SHIFT_R);
    }
}