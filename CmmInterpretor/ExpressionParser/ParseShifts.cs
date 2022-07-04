using System;
using System.Collections.Generic;
using CmmInterpretor.Expressions;
using CmmInterpretor.Extensions;
using CmmInterpretor.Operators.Bitwise;
using CmmInterpretor.Tokens;
using CmmInterpretor.Utils.Exceptions;

namespace CmmInterpretor
{
    internal static partial class ExpressionParser
    {
        private static IExpression ParseShifts(List<Token> tokens, int precedence)
        {
            for (var i = tokens.Count - 1; i >= 0; i--)
            {
                if (tokens[i] is (TokenType.Operator, "<<" or ">>") op)
                {
                    if (i == 0)
                        throw new SyntaxError(op.Start, op.End, "Missing the left part of shift");

                    if (i > tokens.Count - 1)
                        throw new SyntaxError(op.Start, op.End, "Missing the right part of shift");

                    var a = ParseShifts(tokens.GetRange(..i), precedence);
                    var b = Parse(tokens.GetRange((i + 1)..), precedence - 1);

                    return op.Text switch
                    {
                        "<<" => new LeftShift(a, b),
                        ">>" => new RightShift(a, b),
                        _ => throw new Exception()
                    };
                }
            }

            return Parse(tokens, precedence - 1);
        }
    }
}