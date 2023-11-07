using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Expressions.Literals;
using Bloc.Tokens;
using Bloc.Utils.Constants;
using Bloc.Utils.Extensions;

namespace Bloc.Parsers.Steps;

internal sealed class ParseTuples : IParsingStep
{
    private readonly IParsingStep _nextStep;

    public ParseTuples(IParsingStep nextStep)
    {
        _nextStep = nextStep;
    }

    public IExpression Parse(List<Token> tokens)
    {
        var parts = tokens.Split(x => x is SymbolToken(Symbol.COMMA));

        if (parts.Count == 1)
            return _nextStep.Parse(tokens);

        if (parts[^1].Count == 0)
            parts.RemoveAt(parts.Count - 1);

        var expressions = new List<IExpression>();

        foreach (var part in parts)
            expressions.Add(_nextStep.Parse(part));

        return new TupleLiteral(expressions);
    }
}