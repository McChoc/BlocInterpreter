using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Expressions.Switch;
using Bloc.Scanners;
using Bloc.Tokens;
using Bloc.Utils.Constants;
using Bloc.Utils.Exceptions;
using Bloc.Utils.Extensions;

namespace Bloc.Parsers.Steps;

internal sealed class ParseSwitchExpressions : ParsingStep
{
    public ParseSwitchExpressions(ParsingStep nextStep)
        : base(nextStep) { }

    internal override IExpression Parse(List<Token> tokens)
    {
        for (int i = 0; i < tokens.Count; i++)
        {
            if (tokens[i] is not KeywordToken(Keyword.SWITCH) keyword)
                continue;

            if (i == 0)
                throw new SyntaxError(keyword.Start, keyword.End, "Missing value of switch expression");

            var expression = NextStep!.Parse(tokens.GetRange(..i));

            if (i == tokens.Count - 1 || tokens[i + 1] is not BracesToken braces)
                throw new SyntaxError(keyword.Start, keyword.End, "Missing cases of switch expression");

            var cases = new List<IArm>();
            IExpression? @default = null;

            var provider = new TokenCollection(braces.Tokens);

            while (provider.HasNext())
            {
                var caseKeyword = provider.Next();

                switch (caseKeyword)
                {
                    case KeywordToken(Keyword.CASE):
                    {
                        var comparedExpression = GetComparedExpression(provider);
                        var resultExpression = GetResultExpression(provider);
                        cases.Add(new Case(comparedExpression, resultExpression));
                        break;
                    }

                    case KeywordToken(Keyword.MATCH):
                    {
                        var comparedExpression = GetComparedExpression(provider);
                        var resultExpression = GetResultExpression(provider);
                        cases.Add(new Match(comparedExpression, resultExpression));
                        break;
                    }

                    case KeywordToken(Keyword.DEFAULT):
                        if (@default is not null)
                            throw new SyntaxError(caseKeyword.Start, caseKeyword.End, "A switch expression can only have a single default expression");

                        if (!provider.HasNext() || provider.Next() is not SymbolToken(Symbol.COLON))
                            throw new SyntaxError(0, 0, "Unexpected token");

                        @default = GetResultExpression(provider);
                        break;

                    default:
                        throw new SyntaxError(caseKeyword.Start, caseKeyword.End, "Unexpected token");
                }
            }

            var @switch = new SwitchExpression()
            {
                Expression = expression,
                Arms = cases,
                Default = @default
            };

            tokens.RemoveRange(0, i + 2);
            tokens.Insert(0, new ParsedToken(0, 0, @switch));

            return Parse(tokens);
        }

        return NextStep!.Parse(tokens);
    }

    private static IExpression GetComparedExpression(ITokenProvider provider)
    {
        var tokens = new List<Token>();

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
        var tokens = new List<Token>();

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