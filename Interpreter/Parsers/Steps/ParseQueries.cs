using System;
using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Expressions.Operators;
using Bloc.Tokens;
using Bloc.Utils.Constants;
using Bloc.Utils.Exceptions;
using Bloc.Utils.Extensions;

namespace Bloc.Parsers.Steps;

internal sealed class ParseQueries : ParsingStep
{
    public ParseQueries(ParsingStep? nextStep)
        : base(nextStep) { }

    internal override IExpression Parse(List<Token> tokens)
    {
        for (int i = tokens.Count - 1; i >= 0; i--)
        {
            if (IsQuery(tokens[i], out var @operator))
            {
                if (i == 0)
                    throw new SyntaxError(@operator!.Start, @operator.End, "Missing the left part of query");

                if (i == tokens.Count - 1)
                    throw new SyntaxError(@operator!.Start, @operator.End, "Missing the right part of query");

                var left = Parse(tokens.GetRange(..i));
                var right = NextStep!.Parse(tokens.GetRange((i + 1)..));

                return @operator!.Text switch
                {
                    Keyword.SELECT  => new SelectOperator(left, right),
                    Keyword.WHERE   => new WhereOperator(left, right),
                    Keyword.ORDERBY => new OrderbyOperator(left, right),
                    _ => throw new Exception()
                };
            }
        }

        return NextStep!.Parse(tokens);
    }

    private static bool IsQuery(Token token, out TextToken? @operator)
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