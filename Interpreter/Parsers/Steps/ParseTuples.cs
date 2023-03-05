using System.Collections.Generic;
using Bloc.Constants;
using Bloc.Expressions;
using Bloc.Tokens;
using Bloc.Utils.Extensions;

namespace Bloc.Parsers.Steps;

internal sealed class ParseTuples : IParsingStep
{
    public IParsingStep? NextStep { get; init; }

    public ParseTuples(IParsingStep? nextStep)
    {
        NextStep = nextStep;
    }

    public IExpression Parse(List<Token> tokens)
    {
        var parts = tokens.Split(x => x is SymbolToken(Symbol.COMMA));

        if (parts.Count == 1)
            return NextStep!.Parse(tokens);

        var expressions = new List<IExpression>();

        foreach (var part in parts)
            expressions.Add(NextStep!.Parse(part));

        return new TupleLiteral(expressions);
    }
}