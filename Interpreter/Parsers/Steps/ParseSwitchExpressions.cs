using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Expressions.Switch;
using Bloc.Scanners;
using Bloc.Tokens;
using Bloc.Utils.Constants;
using Bloc.Utils.Exceptions;
using Bloc.Utils.Extensions;

namespace Bloc.Parsers.Steps;

internal sealed class ParseSwitchExpressions : IParsingStep
{
    private readonly IParsingStep _nextStep;

    public ParseSwitchExpressions(IParsingStep nextStep)
    {
        _nextStep = nextStep;
    }

    public IExpression Parse(List<IToken> tokens)
    {
        for (int i = 0; i < tokens.Count; i++)
        {
            if (tokens[i] is not KeywordToken(Keyword.SWITCH) keyword)
                continue;

            if (i == 0)
                throw new SyntaxError(keyword.Start, keyword.End, "Missing value of switch expression");

            var expression = _nextStep.Parse(tokens.GetRange(..i));

            if (i == tokens.Count - 1 || tokens[i + 1] is not BracesToken braces)
                throw new SyntaxError(keyword.Start, keyword.End, "Missing cases of switch expression");

            var cases = new List<SwitchExpression.Case>();

            var provider = new TokenCollection(braces.Tokens);

            while (provider.HasNext())
            {
                var comparedExpression = GetComparedExpression(provider);
                var resultExpression = GetResultExpression(provider);
                cases.Add(new SwitchExpression.Case(comparedExpression, resultExpression));
            }

            var @switch = new SwitchExpression()
            {
                Expression = expression,
                Cases = cases
            };

            tokens.RemoveRange(0, i + 2);
            tokens.Insert(0, new ParsedToken(0, 0, @switch));

            return Parse(tokens);
        }

        return _nextStep.Parse(tokens);
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