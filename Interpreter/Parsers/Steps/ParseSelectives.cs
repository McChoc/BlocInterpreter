using System;
using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Expressions.Selectives;
using Bloc.Scanners;
using Bloc.Tokens;
using Bloc.Utils.Constants;
using Bloc.Utils.Exceptions;
using Bloc.Utils.Extensions;

namespace Bloc.Parsers.Steps;

internal sealed class ParseSelectives : IParsingStep
{
    private readonly IParsingStep _nextStep;

    public ParseSelectives(IParsingStep nextStep)
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
                    return ParseSelectiveExpression(tokens, i, BuildSwitchArm);
                case KeywordToken(Keyword.MATCH):
                    return ParseSelectiveExpression(tokens, i, BuildMatchArm);
            }
        }

        return _nextStep.Parse(tokens);
    }

    private SwitchArm BuildSwitchArm(IExpression comparedExpression, IExpression resultExpression)
    {
        return new SwitchArm(comparedExpression, resultExpression);
    }

    private MatchArm BuildMatchArm(IExpression comparedExpression, IExpression resultExpression)
    {
        return new MatchArm(comparedExpression, resultExpression);
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

    private IExpression ParseSelectiveExpression(List<IToken> tokens, int index, Func<IExpression, IExpression, Arm> buildArm)
    {
        var keyword = tokens[index];

        if (index == 0)
            throw new SyntaxError(keyword.Start, keyword.End, "Missing value of selective expression");

        var expression = _nextStep.Parse(tokens.GetRange(..index));

        if (index == tokens.Count - 1 || tokens[index + 1] is not BracesToken braces)
            throw new SyntaxError(keyword.Start, keyword.End, "Missing cases of selective expression");

        var cases = new List<Arm>();

        var provider = new TokenCollection(braces.Tokens);

        while (provider.HasNext())
        {
            var comparedExpression = GetComparedExpression(provider);
            var resultExpression = GetResultExpression(provider);
            var arm = buildArm(comparedExpression, resultExpression);
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
}