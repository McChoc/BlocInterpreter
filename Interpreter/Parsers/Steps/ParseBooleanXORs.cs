using System.Collections.Generic;
using Bloc.Constants;
using Bloc.Exceptions;
using Bloc.Expressions;
using Bloc.Operators;
using Bloc.Tokens;
using Bloc.Utils.Extensions;

namespace Bloc.Parsers.Steps;

internal sealed class ParseBooleanXORs : IParsingStep
{
    public IParsingStep? NextStep { get; init; }

    public ParseBooleanXORs(IParsingStep? nextStep)
    {
        NextStep = nextStep;
    }

    public IExpression Parse(List<Token> tokens)
    {
        for (var i = tokens.Count - 1; i >= 0; i--)
        {
            if (tokens[i] is SymbolToken(Symbol.BOOL_XOR) @operator)
            {
                if (i == 0)
                    throw new SyntaxError(@operator.Start, @operator.End, "Missing the left part of logical XOR");

                if (i == tokens.Count - 1)
                    throw new SyntaxError(@operator.Start, @operator.End, "Missing the right part of logical XOR");

                var left = Parse(tokens.GetRange(..i));
                var right = NextStep!.Parse(tokens.GetRange((i + 1)..));

                return new BooleanXorOperator(left, right);
            }
        }

        return NextStep!.Parse(tokens);
    }
}