using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Expressions.Operators;
using Bloc.Tokens;
using Bloc.Utils.Constants;
using Bloc.Utils.Exceptions;
using Bloc.Utils.Extensions;

namespace Bloc.Parsers.Steps;

internal sealed class ParseConditionals : ParsingStep
{
    public ParseConditionals(ParsingStep? nextStep)
        : base(nextStep) { }

    internal override IExpression Parse(List<Token> tokens)
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
                            var condition = NextStep!.Parse(tokens.GetRange(..i));
                            var consequent = Parse(tokens.GetRange((i + 1)..j));
                            var alternative = Parse(tokens.GetRange((j + 1)..));

                            return new ConditionalOperator(condition, consequent, alternative);
                        }
                    }
                }

                throw new SyntaxError(tokens[^1].Start, tokens[^1].End, "Missing ':'");
            }
        }

        return NextStep!.Parse(tokens);
    }
}