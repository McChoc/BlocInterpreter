using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Expressions.Literals;
using Bloc.Tokens;
using Bloc.Utils.Constants;
using Bloc.Utils.Exceptions;
using Bloc.Utils.Extensions;

namespace Bloc.Parsers.Steps;

internal sealed class ParseRanges : IParsingStep
{
    private readonly IParsingStep _nextStep;

    public ParseRanges(IParsingStep nextStep)
    {
        _nextStep = nextStep;
    }

    public IExpression Parse(List<IToken> tokens)
    {
        bool inclusive = false;
        int firstIndex = -1, secondIndex = -1;

        for (int i = 0; i < tokens.Count; i++)
        {
            switch (tokens[i])
            {
                case SymbolToken(Symbol.COLON):
                    if (firstIndex == -1)
                        firstIndex = i;
                    else if (secondIndex == -1)
                        secondIndex = i;
                    else
                        throw new SyntaxError(tokens[i].Start, tokens[i].End, $"Unexpected symbol '{Symbol.COLON}'");
                    break;

                case SymbolToken(Symbol.INCLUSIVE_RANGE):
                    if (firstIndex != -1)
                        throw new SyntaxError(tokens[i].Start, tokens[i].End, $"Unexpected symbol '{Symbol.INCLUSIVE_RANGE}'");

                    firstIndex = i;
                    inclusive = true;
                    break;
            }
        }

        if (firstIndex == -1)
            return _nextStep.Parse(tokens);

        IExpression? start, end, step;

        start = firstIndex > 0
            ? _nextStep.Parse(tokens.GetRange(..firstIndex))
            : null;

        if (secondIndex == -1)
        {
            end = firstIndex < tokens.Count - 1
                ? _nextStep.Parse(tokens.GetRange((firstIndex + 1)..))
                : null;

            step = null;
        }
        else
        {
            end = secondIndex - firstIndex > 1
                ? _nextStep.Parse(tokens.GetRange((firstIndex + 1)..secondIndex))
                : null;

            step = secondIndex < tokens.Count - 1
                ? _nextStep.Parse(tokens.GetRange((secondIndex + 1)..))
                : null;
        }

        if (inclusive && end is null)
            throw new SyntaxError(0, 0, $"Missing value");

        return new RangeLiteral(start, end, step, inclusive);
    }
}