using System;
using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Expressions.Operators;
using Bloc.Tokens;
using Bloc.Utils.Constants;
using Bloc.Utils.Exceptions;
using Bloc.Utils.Extensions;
using Bloc.Utils.Helpers;

namespace Bloc.Parsers.Steps;

internal sealed class ParseAdditives : ParsingStep
{
    public ParseAdditives(ParsingStep? nextStep)
        : base(nextStep) { }

    internal override IExpression Parse(List<Token> tokens)
    {
        for (var i = tokens.Count - 1; i >= 0; i--)
        {
            if (IsAdditive(tokens[i], out var @operator) && OperatorHelper.IsBinary(tokens, i))
            {
                if (i == tokens.Count - 1)
                    throw new SyntaxError(@operator!.Start, @operator.End, "Missing right part of additive");

                var left = Parse(tokens.GetRange(..i));
                var right = NextStep!.Parse(tokens.GetRange((i + 1)..));

                return @operator!.Text switch
                {
                    Symbol.PLUS => new AdditionOperator(left, right),
                    Symbol.MINUS => new SubstractionOperator(left, right),
                    _ => throw new Exception()
                };
            }
        }

        return NextStep!.Parse(tokens);
    }

    private static bool IsAdditive(Token token, out TextToken? @operator)
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