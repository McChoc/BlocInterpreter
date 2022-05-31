using CmmInterpretor.Utils.Exceptions;
using CmmInterpretor.Expressions;
using CmmInterpretor.Extensions;
using CmmInterpretor.Operators.Arithmetic;
using CmmInterpretor.Tokens;
using System.Collections.Generic;

namespace CmmInterpretor
{
    internal static partial class ExpressionParser
    {
        private static IExpression ParseAdditives(List<Token> tokens, int precedence)
        {
            for (int i = tokens.Count - 1; i >= 0; i--)
            {
                if (tokens[i] is (TokenType.Operator, "+" or "-") op)
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
                        throw new SyntaxError(op.Start, op.End, "Missing right part of additive");

                    var a = ParseAdditives(tokens.GetRange(..i), precedence);
                    var b = Parse(tokens.GetRange((i + 1)..), precedence - 1);

                    return op.Text switch
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
