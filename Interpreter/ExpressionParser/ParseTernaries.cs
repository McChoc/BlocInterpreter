using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Extensions;
using Bloc.Operators;
using Bloc.Tokens;
using Bloc.Utils;

namespace Bloc;

internal static partial class ExpressionParser
{
    private static IExpression ParseTernaries(List<Token> tokens, int precedence)
    {
        for (var i = 0; i < tokens.Count; i++)
        {
            if (tokens[i] is (TokenType.Symbol, Symbol.QUESTION))
            {
                var depth = 0;

                for (var j = i; j < tokens.Count; j++)
                {
                    if (tokens[j] is (TokenType.Symbol, Symbol.QUESTION))
                        depth++;

                    if (tokens[j] is (TokenType.Symbol, Symbol.COLON))
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