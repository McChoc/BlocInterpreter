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

internal sealed class ParseQueries : IParsingStep
{
    private readonly IParsingStep _nextStep;

    public ParseQueries(IParsingStep nextStep)
    {
        _nextStep = nextStep;
    }

    public IExpression Parse(List<IToken> tokens)
    {
        for (int i = tokens.Count - 1; i >= 0; i--)
        {
            if (IsQuery(tokens[i], out var @operator))
            {
                if (i == 0)
                    throw new SyntaxError(@operator.Start, @operator.End, "Missing the left part of query");

                if (i == tokens.Count - 1)
                    throw new SyntaxError(@operator.Start, @operator.End, "Missing the right part of query");

                var left = Parse(tokens.GetRange(..i));
                var right = _nextStep.Parse(tokens.GetRange((i + 1)..));

                return @operator.Text switch
                {
                    Keyword.SELECT  => new SelectOperator(left, right),
                    Keyword.WHERE   => new WhereOperator(left, right),
                    Keyword.ORDERBY => new OrderbyOperator(left, right),
                    _ => throw new Exception()
                };
            }
        }

        return _nextStep.Parse(tokens);
    }

    private static bool IsQuery(IToken token, [NotNullWhen(true)] out TextToken? @operator)
    {
        if (token is KeywordToken(Keyword.SELECT or Keyword.WHERE or Keyword.ORDERBY))
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