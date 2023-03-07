﻿using System.Collections.Generic;
using Bloc.Constants;
using Bloc.Exceptions;
using Bloc.Expressions;
using Bloc.Tokens;
using Bloc.Utils.Extensions;

namespace Bloc.Parsers.Steps;

internal sealed class ParseRanges : IParsingStep
{
    public IParsingStep? NextStep { get; init; }

    public ParseRanges(IParsingStep? nextStep)
    {
        NextStep = nextStep;
    }

    public IExpression Parse(List<Token> tokens)
    {
        var parts = tokens.Split(x => x is SymbolToken(Symbol.COLON));

        if (parts.Count == 1)
            return NextStep!.Parse(parts[0]);

        if (parts.Count == 2)
        {
            var start = parts[0].Count > 0 ? NextStep!.Parse(parts[0]) : null;
            var end = parts[1].Count > 0 ? NextStep!.Parse(parts[1]) : null;

            return new RangeLiteral(start, end, null);
        }

        if (parts.Count == 3)
        {
            var start = parts[0].Count > 0 ? NextStep!.Parse(parts[0]) : null;
            var end = parts[1].Count > 0 ? NextStep!.Parse(parts[1]) : null;
            var step = parts[2].Count > 0 ? NextStep!.Parse(parts[2]) : null;

            return new RangeLiteral(start, end, step);
        }

        throw new SyntaxError(tokens[0].Start, tokens[^1].End, $"Unexpected symbol '{Symbol.COLON}'");
    }
}