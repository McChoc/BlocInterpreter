using System;
using System.Collections.Generic;
using Bloc.Constants;
using Bloc.Exceptions;
using Bloc.Expressions;
using Bloc.Operators;
using Bloc.Tokens;
using Bloc.Utils.Extensions;

namespace Bloc.Parsers.Steps;

internal sealed class ParseShifts : IParsingStep
{
    public IParsingStep? NextStep { get; init; }

    public ParseShifts(IParsingStep? nextStep)
    {
        NextStep = nextStep;
    }

    public IExpression Parse(List<Token> tokens)
    {
        for (var i = tokens.Count - 1; i >= 0; i--)
        {
            if (IsShift(tokens[i], out var @operator))
            {
                if (i == 0)
                    throw new SyntaxError(@operator!.Start, @operator.End, "Missing the left part of shift");

                if (i > tokens.Count - 1)
                    throw new SyntaxError(@operator!.Start, @operator.End, "Missing the right part of shift");

                var left = Parse(tokens.GetRange(..i));
                var right = NextStep!.Parse(tokens.GetRange((i + 1)..));

                return @operator!.Text switch
                {
                    Symbol.SHIFT_L => new LeftShiftOperator(left, right),
                    Symbol.SHIFT_R => new RightShiftOperator(left, right),
                    _ => throw new Exception()
                };
            }
        }

        return NextStep!.Parse(tokens);
    }

    private static bool IsShift(Token token, out TextToken? @operator)
    {
        if (token is SymbolToken(Symbol.SHIFT_L or Symbol.SHIFT_R))
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