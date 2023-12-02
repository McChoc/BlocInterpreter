using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Expressions.Operators;
using Bloc.Funcs;
using Bloc.Tokens;
using Bloc.Utils.Constants;
using Bloc.Utils.Exceptions;
using Bloc.Utils.Extensions;

namespace Bloc.Parsers.Steps;

internal sealed class ParsePrimaries : IParsingStep
{
    private readonly IParsingStep _nextStep;

    public ParsePrimaries(IParsingStep nextStep)
    {
        _nextStep = nextStep;
    }

    public IExpression Parse(List<IToken> tokens)
    {
        return tokens switch
        {
            [Token token] => _nextStep.Parse(new() { token }),
            [WordToken(Keyword.GLOBAL), SymbolToken(Symbol.DBL_COLON), INamedIdentifierToken token] => new GlobalOperator(_nextStep.Parse(new() { token })),
            [WordToken(Keyword.TOPLVL), SymbolToken(Symbol.DBL_COLON), INamedIdentifierToken token] => new ToplvlOperator(_nextStep.Parse(new() { token })),
            [WordToken(Keyword.OUTER), SymbolToken(Symbol.DBL_COLON), INamedIdentifierToken token] => new OuterOperator(_nextStep.Parse(new() { token })),
            [WordToken(Keyword.PARAM), SymbolToken(Symbol.DBL_COLON), INamedIdentifierToken token] => new ParamOperator(_nextStep.Parse(new() { token })),
            [WordToken(Keyword.LOCAL), SymbolToken(Symbol.DBL_COLON), INamedIdentifierToken token] => new LocalOperator(_nextStep.Parse(new() { token })),
            [.., _, SymbolToken(Symbol.DOT), INamedIdentifierToken token] => new MemberAccessOperator(Parse(tokens.GetRange(..^2)), token.GetIdentifier()),
            [.., _, BracketsToken brackets] => new IndexerOperator(Parse(tokens.GetRange(..^1)), ParseIndexingArguments(brackets.Tokens)),
            [.., _, ParenthesesToken parentheses] => new InvocationOperator(Parse(tokens.GetRange(..^1)), ParseInvocationArguments(parentheses.Tokens)),
            _ => throw new SyntaxError(tokens[0].Start, tokens[^1].End, "Unexpected token")
        };
    }

    private static List<IndexerOperator.Argument> ParseIndexingArguments(List<IToken> tokens)
    {
        var arguments = new List<IndexerOperator.Argument>();

        if (tokens.Count > 0)
        {
            foreach (var part in tokens.Split(x => x is SymbolToken(Symbol.COMMA)))
            {
                var argument = part switch
                {
                    [] => throw new SyntaxError(0, 0, "Unexpected token"),
                    [SymbolToken(Symbol.STAR), ..] => new IndexerOperator.Argument(ExpressionParser.Parse(part.GetRange(1..)), true),
                    _ => new IndexerOperator.Argument(ExpressionParser.Parse(part), false),
                };

                arguments.Add(argument);
            }
        }

        return arguments;
    }

    private static List<InvocationOperator.Argument> ParseInvocationArguments(List<IToken> tokens)
    {
        var arguments = new List<InvocationOperator.Argument>();

        if (tokens.Count > 0)
        {
            foreach (var part in tokens.Split(x => x is SymbolToken(Symbol.COMMA)))
            {
                var argument = part switch
                {
                    [] => new InvocationOperator.Argument(null, null, ArgumentType.Positional),
                    [SymbolToken(Symbol.STAR), ..] => new InvocationOperator.Argument(null, ExpressionParser.Parse(part.GetRange(1..)), ArgumentType.UnpackedArray),
                    [SymbolToken(Symbol.DBL_STAR), ..] => new InvocationOperator.Argument(null, ExpressionParser.Parse(part.GetRange(1..)), ArgumentType.UnpackedStruct),
                    [INamedIdentifierToken token, SymbolToken(Symbol.EXCL)] => new InvocationOperator.Argument(token.GetIdentifier(), ExpressionParser.Parse(part.GetRange(..1)), ArgumentType.Keyword),
                    [INamedIdentifierToken token, SymbolToken(Symbol.COLON), _, ..] => new InvocationOperator.Argument(token.GetIdentifier(), ExpressionParser.Parse(part.GetRange(2..)), ArgumentType.Keyword),
                    _ => new InvocationOperator.Argument(null, ExpressionParser.Parse(part), ArgumentType.Positional)
                };

                arguments.Add(argument);
            }
        }

        return arguments;
    }
}