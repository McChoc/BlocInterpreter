using CmmInterpretor.Utils.Exceptions;
using CmmInterpretor.Expressions;
using CmmInterpretor.Extensions;
using CmmInterpretor.Operators.Arithmetic;
using CmmInterpretor.Tokens;
using System.Collections.Generic;

namespace CmmInterpretor
{
    public static partial class ExpressionParser
    {
        private static IExpression ParseExponentials(List<Token> tokens, int precedence)
        {
            for (int i = tokens.Count - 1; i >= 0; i--)
            {
                if (tokens[i] is { type: TokenType.Operator, value: "**" or "//" or "%%" })
                {
                    if (i == 0)
                        throw new SyntaxError("Missing the left part of exponential");

                    if (i > tokens.Count - 1)
                        throw new SyntaxError("Missing the right part of exponential");

                    var a = ParseExponentials(tokens.GetRange(..i), precedence);
                    var b = Parse(tokens.GetRange((i + 1)..), precedence - 1);

                    return tokens[i].value switch
                    {
                        "**" => new Power(a, b),
                        "//" => new Root(a, b),
                        "%%" => new Logarithm(a, b),
                        _ => throw new System.Exception()
                    };
                }
            }

            return Parse(tokens, precedence - 1);
        }
    }
}
