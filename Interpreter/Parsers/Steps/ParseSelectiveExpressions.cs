using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Expressions.Operators;
using Bloc.Scanners;
using Bloc.Tokens;
using Bloc.Utils.Constants;
using Bloc.Utils.Exceptions;
using Bloc.Utils.Extensions;

namespace Bloc.Parsers.Steps;

internal sealed class ParseSelectiveExpressions : IParsingStep
{
    private readonly IParsingStep _nextStep;

    public ParseSelectiveExpressions(IParsingStep nextStep)
    {
        _nextStep = nextStep;
    }

    public IExpression Parse(List<IToken> tokens)
    {
        for (int i = 0; i < tokens.Count; i++)
        {
            switch (tokens[i])
            {
                case KeywordToken(Keyword.SWITCH):
                    return ParseSelectiveExpression<SelectiveExpression.SwitchCase>(tokens, i);
                case KeywordToken(Keyword.MATCH):
                    return ParseSelectiveExpression<SelectiveExpression.MatchCase>(tokens, i);
            }
        }

        return _nextStep.Parse(tokens);
    }

    private IExpression ParseSelectiveExpression<T>(List<IToken> tokens, int index)
        where T : SelectiveExpression.Case, new()
    {
        var keyword = tokens[index];

        if (index == 0)
            throw new SyntaxError(keyword.Start, keyword.End, "Missing value of selective expression");

        var expression = _nextStep.Parse(tokens.GetRange(..index));

        if (index == tokens.Count - 1 || tokens[index + 1] is not BracesToken braces)
            throw new SyntaxError(keyword.Start, keyword.End, "Missing cases of selective expression");

        var cases = new List<SelectiveExpression.Case>();

        var provider = new TokenCollection(braces.Tokens);

        while (provider.HasNext())
        {
            var comparedExpression = GetComparedExpression(provider);
            var resultExpression = GetResultExpression(provider);

            var arm = new T()
            {
                ComparedExpression = comparedExpression,
                ResultExpression = resultExpression
            };

            cases.Add(arm);
        }

        var @switch = new SelectiveExpression()
        {
            Expression = expression,
            Cases = cases
        };

        tokens.RemoveRange(0, index + 2);
        tokens.Insert(0, new ParsedToken(0, 0, @switch));

        return Parse(tokens);
    }

    private static IExpression GetComparedExpression(ITokenProvider provider)
    {
        var tokens = new List<IToken>();

        while (provider.HasNext())
        {
            var token = provider.Next();

            if (token is SymbolToken(Symbol.COLON))
                break;

            tokens.Add(token);
        }

        return ExpressionParser.Parse(tokens);
    }

    private static IExpression GetResultExpression(ITokenProvider provider)
    {
        var tokens = new List<IToken>();

        while (provider.HasNext())
        {
            var token = provider.Next();

            if (token is SymbolToken(Symbol.COMMA))
                break;

            tokens.Add(token);
        }

        return ExpressionParser.Parse(tokens);
    }
}