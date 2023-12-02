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

internal sealed class ParseCoalescings : IParsingStep
{
    private readonly IParsingStep _nextStep;

    public ParseCoalescings(IParsingStep nextStep)
    {
        _nextStep = nextStep;
    }

    public IExpression Parse(List<IToken> tokens)
    {
        for (int i = tokens.Count - 1; i >= 0; i--)
        {
            if (IsCoalescing(tokens[i], out var @operator))
            {
                if (i == 0)
                    throw new SyntaxError(@operator.Start, @operator.End, "Missing the left part of coalescing");

                if (i == tokens.Count - 1)
                    throw new SyntaxError(@operator.Start, @operator.End, "Missing the right part of coalescing");

                var left = Parse(tokens.GetRange(..i));
                var right = _nextStep.Parse(tokens.GetRange((i + 1)..));

                return @operator.Text switch
                {
                    Symbol.DBL_QUESTION => new NullCoalescingOperator(left, right),
                    Symbol.TPL_QUESTION => new VoidCoalescingOperator(left, right),
                    _ => throw new Exception()
                };
            }
        }

        return _nextStep.Parse(tokens);
    }

    private static bool IsCoalescing(IToken token, [NotNullWhen(true)] out TextToken? @operator)
    {
        if (token is SymbolToken(Symbol.DBL_QUESTION or Symbol.TPL_QUESTION))
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