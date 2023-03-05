using System.Collections.Generic;
using Bloc.Constants;
using Bloc.Expressions;
using Bloc.Operators;
using Bloc.Tokens;
using Bloc.Utils.Extensions;

namespace Bloc;

internal static partial class ExpressionParser
{
    private static IExpression ParseConditionals(List<Token> tokens, int precedence)
    {
        for (var i = 0; i < tokens.Count; i++)
        {
            if (tokens[i] is SymbolToken(Symbol.QUESTION))
            {
                var depth = 0;

                for (var j = i; j < tokens.Count; j++)
                {
                    if (tokens[j] is SymbolToken(Symbol.QUESTION))
                        depth++;

                    if (tokens[j] is SymbolToken(Symbol.COLON))
                    {
                        depth--;

                        if (depth == 0)
                        {
                            var condition = Parse(tokens.GetRange(..i), precedence - 1);
                            var consequent = ParseConditionals(tokens.GetRange((i + 1)..j), precedence);
                            var alternative = ParseConditionals(tokens.GetRange((j + 1)..), precedence);

                            return new ConditionalOperator(condition, consequent, alternative);
                        }
                    }
                }
            }
        }

        return Parse(tokens, precedence - 1);
    }
}