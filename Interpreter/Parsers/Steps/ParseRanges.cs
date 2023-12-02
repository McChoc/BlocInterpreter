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
        int firstIndex = -1, secondIndex = -1;
        bool inclusiveStart = true, inclusiveStop = true;

        for (int i = 0; i < tokens.Count; i++)
        {
            switch (tokens[i])
            {
                case SymbolToken(Symbol.RANGE_INC_INC):
                    if (firstIndex != -1)
                        throw new SyntaxError(tokens[i].Start, tokens[i].End, $"Unexpected symbol '{Symbol.RANGE_INC_INC}'");

                    firstIndex = i;
                    break;

                case SymbolToken(Symbol.RANGE_INC_EXC):
                    if (firstIndex != -1)
                        throw new SyntaxError(tokens[i].Start, tokens[i].End, $"Unexpected symbol '{Symbol.RANGE_INC_EXC}'");

                    firstIndex = i;
                    inclusiveStop = false;
                    break;

                case SymbolToken(Symbol.RANGE_EXC_INC):
                    if (firstIndex != -1)
                        throw new SyntaxError(tokens[i].Start, tokens[i].End, $"Unexpected symbol '{Symbol.RANGE_EXC_INC}'");

                    firstIndex = i;
                    inclusiveStart = false;
                    break;

                case SymbolToken(Symbol.RANGE_EXC_EXC):
                    if (firstIndex != -1)
                        throw new SyntaxError(tokens[i].Start, tokens[i].End, $"Unexpected symbol '{Symbol.RANGE_EXC_EXC}'");

                    firstIndex = i;
                    inclusiveStart = false;
                    inclusiveStop = false;
                    break;

                case SymbolToken(Symbol.COLON):
                    if (firstIndex == -1 || secondIndex != -1)
                        throw new SyntaxError(tokens[i].Start, tokens[i].End, $"Unexpected symbol '{Symbol.COLON}'");

                    secondIndex = i;
                    break;
            }
        }

        if (firstIndex == -1)
            return _nextStep.Parse(tokens);

        RangeLiteral.Index start, stop;
        IExpression? step;

        start = firstIndex > 0
            ? new(_nextStep.Parse(tokens.GetRange(..firstIndex)), inclusiveStart)
            : new(null, inclusiveStart);

        if (secondIndex == -1)
        {
            stop = firstIndex < tokens.Count - 1
                ? new(_nextStep.Parse(tokens.GetRange((firstIndex + 1)..)), inclusiveStop)
                : new(null, inclusiveStop);

            step = null;
        }
        else
        {
            stop = secondIndex - firstIndex > 1
                ? new(_nextStep.Parse(tokens.GetRange((firstIndex + 1)..secondIndex)), inclusiveStop)
                : new(null, inclusiveStop);

            step = secondIndex < tokens.Count - 1
                ? _nextStep.Parse(tokens.GetRange((secondIndex + 1)..))
                : null;
        }

        return new RangeLiteral(start, stop, step);
    }
}