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
        private static IExpression ParseMultiplicatives(List<Token> tokens, int precedence)
        {
            for (int i = tokens.Count - 1; i >= 0; i--)
            {
                if (tokens[i] is { type: TokenType.Operator, value: "*" or "/" or "%" })
                {
                    if (i == 0)
                        throw new SyntaxError("Missing the left part of multiplicative");

                    if (i > tokens.Count - 1)
                        throw new SyntaxError("Missing the right part of multiplicative");

                    var a = ParseMultiplicatives(tokens.GetRange(..i), precedence);
                    var b = Parse(tokens.GetRange((i + 1)..), precedence - 1);

                    return tokens[i].value switch
                    {
                        "*" => new Multiplication(a, b),
                        "/" => new Division(a, b),
                        "%" => new Remainder(a, b),
                        _ => throw new System.Exception()
                    };
                }
            }

            return Parse(tokens, precedence - 1);
        }
    }
}
