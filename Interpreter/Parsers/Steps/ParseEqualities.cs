using System.Collections.Generic;
using Bloc.Constants;
using Bloc.Exceptions;
using Bloc.Expressions;
using Bloc.Operators;
using Bloc.Tokens;
using Bloc.Utils.Extensions;

namespace Bloc.Parsers.Steps;

internal sealed class ParseEqualities : IParsingStep
{
    public IParsingStep? NextStep { get; init; }

    public ParseEqualities(IParsingStep? nextStep)
    {
        NextStep = nextStep;
    }

    public IExpression Parse(List<Token> tokens)
    {
        for (var i = tokens.Count - 1; i >= 0; i--)
        {
            if (IsEquality(tokens[i], out var @operator))
            {
                if (i == 0)
                    throw new SyntaxError(@operator!.Start, @operator.End, "Missing the left part of equality");

                if (i > tokens.Count - 1)
                    throw new SyntaxError(@operator!.Start, @operator.End, "Missing the right part of equality");

                var left = Parse(tokens.GetRange(..i));
                var right = NextStep!.Parse(tokens.GetRange((i + 1)..));

                return @operator!.Text == Symbol.IS_EQUAL
                    ? new EqualOperator(left, right)
                    : new NotEqualOperator(left, right);
            }
        }

        return NextStep!.Parse(tokens);
    }

    private static bool IsEquality(Token token, out TextToken? @operator)
    {
        if (token is SymbolToken(Symbol.IS_EQUAL or Symbol.NOT_EQUAL_0 or Symbol.NOT_EQUAL_1 or Symbol.NOT_EQUAL_2))
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