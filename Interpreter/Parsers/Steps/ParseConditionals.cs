using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Expressions.Operators;
using Bloc.Tokens;
using Bloc.Utils.Constants;
using Bloc.Utils.Exceptions;
using Bloc.Utils.Extensions;

namespace Bloc.Parsers.Steps;

internal sealed class ParseConditionals : IParsingStep
{
    private readonly IParsingStep _nextStep;

    public ParseConditionals(IParsingStep nextStep)
    {
        _nextStep = nextStep;
    }

    public IExpression Parse(List<IToken> tokens)
    {
        int firstIndex = -1, secondIndex = -1;

        for (int i = 0; i < tokens.Count; i++)
        {
            if (tokens[i] is SymbolToken(Symbol.QUESTION))
            {
                firstIndex = i;
                break;
            }
        }

        if (firstIndex == -1)
            return _nextStep.Parse(tokens);

        int depth = 1;

        for (int i = firstIndex + 1; i < tokens.Count; i++)
        {
            if (tokens[i] is SymbolToken(Symbol.QUESTION))
                depth++;

            if (tokens[i] is SymbolToken(Symbol.COLON))
            {
                switch (--depth)
                {
                    case 0:
                        secondIndex = i;
                        break;
                    case < 0:
                        throw new SyntaxError(tokens[i].Start, tokens[i].End, "Unexpected symbol ':'");
                }
            }
        }

        if (secondIndex == -1)
            throw new SyntaxError(tokens[^1].Start, tokens[^1].End, "Missing ':'");

        var condition = _nextStep.Parse(tokens.GetRange(..firstIndex));
        var consequent = Parse(tokens.GetRange((firstIndex + 1)..secondIndex));
        var alternative = Parse(tokens.GetRange((secondIndex + 1)..));

        return new ConditionalOperator(condition, consequent, alternative);
    }
}