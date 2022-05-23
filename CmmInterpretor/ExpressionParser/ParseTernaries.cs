using CmmInterpretor.Expressions;
using CmmInterpretor.Extensions;
using CmmInterpretor.Operators.Boolean;
using CmmInterpretor.Tokens;
using System.Collections.Generic;

namespace CmmInterpretor
{
    public static partial class ExpressionParser
    {
        private static IExpression ParseTernaries(List<Token> tokens, int precedence)
        {
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i] is { type: TokenType.Operator, value: "?" })
                {
                    int depth = 0;

                    for (int j = i; j < tokens.Count; j++)
                    {
                        if (tokens[j] is { type: TokenType.Operator, value: "?" })
                            depth++;

                        if (tokens[j] is { type: TokenType.Operator, value: ":" })
                        {
                            depth--;

                            if (depth == 0)
                            {
                                var condition = Parse(tokens.GetRange(..i), precedence - 1);
                                var consequent = ParseTernaries(tokens.GetRange((i + 1)..j), precedence);
                                var alternative = ParseTernaries(tokens.GetRange((j + 1)..), precedence);

                                return new Conditional(condition, consequent, alternative);
                            }
                        }
                    }
                }
            }

            return Parse(tokens, precedence - 1);
        }
    }
}
