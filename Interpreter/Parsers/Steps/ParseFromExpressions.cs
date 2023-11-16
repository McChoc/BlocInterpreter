using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Expressions.Operators;
using Bloc.Tokens;
using Bloc.Utils.Constants;
using Bloc.Utils.Exceptions;
using Bloc.Utils.Extensions;

namespace Bloc.Parsers.Steps;

internal sealed class ParseFromExpressions : IParsingStep
{
    private readonly IParsingStep _nextStep;

    public ParseFromExpressions(IParsingStep nextStep)
    {
        _nextStep = nextStep;
    }

    public IExpression Parse(List<IToken> tokens)
    {
        for (int i = tokens.Count - 1; i >= 0; i--)
        {
            if (tokens[i] is KeywordToken(Keyword.FROM) @operator)
            {
                if (i == 0)
                    throw new SyntaxError(@operator.Start, @operator.End, "Missing the left part of from expression");

                if (i == tokens.Count - 1)
                    throw new SyntaxError(@operator.Start, @operator.End, "Missing the right part of from expression");


                var identifier = IdentifierParser.Parse(tokens.GetRange(..i));
                var expression = _nextStep.Parse(tokens.GetRange((i + 1)..));

                return new FromOperator(identifier, expression);
            }
        }

        return _nextStep.Parse(tokens);
    }
}