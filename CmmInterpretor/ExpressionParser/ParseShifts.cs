using CmmInterpretor.Utils.Exceptions;
using CmmInterpretor.Expressions;
using CmmInterpretor.Extensions;
using CmmInterpretor.Operators.Bitwise;
using CmmInterpretor.Tokens;
using System.Collections.Generic;

namespace CmmInterpretor
{
    public static partial class ExpressionParser
    {
        private static IExpression ParseShifts(List<Token> tokens, int precedence)
        {
            for (int i = tokens.Count - 1; i >= 0; i--)
            {
                if (tokens[i] is {type: TokenType.Operator, value: "<<" or ">>" })
                {
                    if (i == 0)
                        throw new SyntaxError("Missing the left part of shift");

                    if (i > tokens.Count - 1)
                        throw new SyntaxError("Missing the right part of shift");

                    var a = ParseShifts(tokens.GetRange(..i), precedence);
                    var b = Parse(tokens.GetRange((i + 1)..), precedence - 1);

                    return tokens[i].value switch
                    {
                        "<<" => new LeftShift(a, b),
                        ">>" => new RightShift(a, b),
                        _ => throw new System.Exception()
                    };
                }
            }

            return Parse(tokens, precedence - 1);
        }
    }
}
