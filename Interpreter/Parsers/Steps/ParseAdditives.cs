using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Bloc.Expressions;
using Bloc.Expressions.Operators;
using Bloc.Tokens;
using Bloc.Utils.Constants;
using Bloc.Utils.Exceptions;
using Bloc.Utils.Extensions;
using Bloc.Utils.Helpers;

namespace Bloc.Parsers.Steps;

internal sealed class ParseAdditives : IParsingStep
{
    private readonly IParsingStep _nextStep;

    public ParseAdditives(IParsingStep nextStep)
    {
        _nextStep = nextStep;
    }

    public IExpression Parse(List<Token> tokens)
    {
        for (int i = tokens.Count - 1; i >= 0; i--)
        {
            if (IsAdditive(tokens[i], out var @operator) && OperatorHelper.IsBinary(tokens, i))
            {
                if (i == tokens.Count - 1)
                    throw new SyntaxError(@operator.Start, @operator.End, "Missing right part of additive");

                var left = Parse(tokens.GetRange(..i));
                var right = _nextStep.Parse(tokens.GetRange((i + 1)..));

                return @operator.Text switch
                {
                    Symbol.PLUS => new AdditionOperator(left, right),
                    Symbol.MINUS => new SubstractionOperator(left, right),
                    _ => throw new Exception()
                };
            }
        }

        return _nextStep.Parse(tokens);
    }

    private static bool IsAdditive(Token token, [NotNullWhen(true)] out TextToken? @operator)
    {
        if (token is SymbolToken(Symbol.PLUS or Symbol.MINUS))
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