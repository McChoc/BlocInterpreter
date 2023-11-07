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

internal sealed class ParseMultiplicatives : IParsingStep
{
    private readonly IParsingStep _nextStep;

    public ParseMultiplicatives(IParsingStep nextStep)
    {
        _nextStep = nextStep;
    }

    public IExpression Parse(List<Token> tokens)
    {
        for (int i = tokens.Count - 1; i >= 0; i--)
        {
            if (IsMultiplicative(tokens[i], out var @operator))
            {
                if (i == 0)
                    throw new SyntaxError(@operator.Start, @operator.End, "Missing the left part of multiplicative");

                if (i == tokens.Count - 1)
                    throw new SyntaxError(@operator.Start, @operator.End, "Missing the right part of multiplicative");

                var left = Parse(tokens.GetRange(..i));
                var right = _nextStep.Parse(tokens.GetRange((i + 1)..));

                return @operator.Text switch
                {
                    Symbol.TIMES =>     new MultiplicationOperator(left, right),
                    Symbol.SLASH =>     new DivisionOperator(left, right),
                    Symbol.REMAINDER => new RemainderOperator(left, right),
                    Symbol.MODULO =>    new ModuloOperator(left, right),
                    _ => throw new Exception()
                };
            }
        }

        return _nextStep.Parse(tokens);
    }

    private static bool IsMultiplicative(Token token, [NotNullWhen(true)] out TextToken? @operator)
    {
        if (token is SymbolToken(
            Symbol.TIMES or
            Symbol.SLASH or
            Symbol.REMAINDER or
            Symbol.MODULO))
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