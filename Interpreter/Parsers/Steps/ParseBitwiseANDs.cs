using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Expressions.Operators;
using Bloc.Tokens;
using Bloc.Utils.Constants;
using Bloc.Utils.Exceptions;
using Bloc.Utils.Extensions;

namespace Bloc.Parsers.Steps;

internal sealed class ParseBitwiseANDs : IParsingStep
{
    private readonly IParsingStep _nextStep;

    public ParseBitwiseANDs(IParsingStep nextStep)
    {
        _nextStep = nextStep;
    }

    public IExpression Parse(List<IToken> tokens)
    {
        for (int i = tokens.Count - 1; i >= 0; i--)
        {
            if (tokens[i] is SymbolToken(Symbol.AMP) @operator)
            {
                if (i == 0)
                    throw new SyntaxError(@operator.Start, @operator.End, "Missing the left part of logical AND");

                if (i == tokens.Count - 1)
                    throw new SyntaxError(@operator.Start, @operator.End, "Missing the right part of logical AND");

                var left = Parse(tokens.GetRange(..i));
                var right = _nextStep.Parse(tokens.GetRange((i + 1)..));

                return new BitwiseAndOperator(left, right);
            }
        }

        return _nextStep.Parse(tokens);
    }
}