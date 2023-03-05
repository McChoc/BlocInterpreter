using System.Collections.Generic;
using Bloc.Constants;
using Bloc.Exceptions;
using Bloc.Expressions;
using Bloc.Operators;
using Bloc.Tokens;
using Bloc.Utils.Extensions;

namespace Bloc.Parsers.Steps;

internal sealed class ParseBooleanANDs : IParsingStep
{
    public IParsingStep? NextStep { get; init; }

    public ParseBooleanANDs(IParsingStep? nextStep)
    {
        NextStep = nextStep;
    }

    public IExpression Parse(List<Token> tokens)
    {
        for (var i = tokens.Count - 1; i >= 0; i--)
        {
            if (tokens[i] is SymbolToken(Symbol.BOOL_AND) @operator)
            {
                if (i == 0)
                    throw new SyntaxError(@operator.Start, @operator.End, "Missing the left part of logical AND");

                if (i == tokens.Count - 1)
                    throw new SyntaxError(@operator.Start, @operator.End, "Missing the right part of logical AND");

                var left = Parse(tokens.GetRange(..i));
                var right = NextStep!.Parse(tokens.GetRange((i + 1)..));

                return new BooleanAndOperator(left, right);
            }
        }

        return NextStep!.Parse(tokens);
    }
}