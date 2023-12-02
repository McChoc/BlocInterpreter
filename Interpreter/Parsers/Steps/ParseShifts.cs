using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Bloc.Expressions;
using Bloc.Expressions.Operators;
using Bloc.Tokens;
using Bloc.Utils.Constants;
using Bloc.Utils.Exceptions;
using Bloc.Utils.Extensions;

namespace Bloc.Parsers.Steps;

internal sealed class ParseShifts : IParsingStep
{
    private readonly IParsingStep _nextStep;

    public ParseShifts(IParsingStep nextStep)
    {
        _nextStep = nextStep;
    }

    public IExpression Parse(List<IToken> tokens)
    {
        for (int i = tokens.Count - 1; i >= 0; i--)
        {
            if (IsShift(tokens[i], out var @operator))
            {
                if (i == 0)
                    throw new SyntaxError(@operator.Start, @operator.End, "Missing the left part of shift");

                if (i > tokens.Count - 1)
                    throw new SyntaxError(@operator.Start, @operator.End, "Missing the right part of shift");

                var left = Parse(tokens.GetRange(..i));
                var right = _nextStep.Parse(tokens.GetRange((i + 1)..));

                return @operator.Text switch
                {
                    Symbol.L_SHIFT => new LeftShiftOperator(left, right),
                    Symbol.R_SHIFT => new RightShiftOperator(left, right),
                    _ => throw new Exception()
                };
            }
        }

        return _nextStep.Parse(tokens);
    }

    private static bool IsShift(IToken token, [NotNullWhen(true)] out TextToken? @operator)
    {
        if (token is SymbolToken(Symbol.L_SHIFT or Symbol.R_SHIFT))
        {
            @operator = (TextToken)token;
            return true;
        }
        else
        {
            @operator = null;
            return false;
        }
    }
}