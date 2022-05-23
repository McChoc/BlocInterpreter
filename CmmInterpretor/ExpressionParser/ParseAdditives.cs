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
        private static IExpression ParseAdditives(List<Token> tokens, int precedence)
        {
            for (int i = tokens.Count - 1; i >= 0; i--)
            {
                if (tokens[i] is { type: TokenType.Operator, value: "+" or "-" })
                {
                    if (i == tokens.Count - 1)
                        throw new SyntaxError("Missing right part of additive");

                    if (i == 0)
                        continue;

                    if (i == 1 &&
                        tokens[0].type is TokenType.Operator or TokenType.Keyword)
                        continue;

                    if (i >= 2 &&
                        tokens[i - 1].type is TokenType.Operator or TokenType.Keyword &&
                        tokens[i - 2].type is not TokenType.Identifier)
                        continue;

                    var a = ParseAdditives(tokens.GetRange(..i), precedence);
                    var b = Parse(tokens.GetRange((i + 1)..), precedence - 1);

                    return tokens[i].value switch
                    {
                        "+" => new Addition(a, b),
                        "-" => new Substraction(a, b),
                        _ => throw new System.Exception()
                    };
                }
            }

            return Parse(tokens, precedence - 1);
        }
    }
}
