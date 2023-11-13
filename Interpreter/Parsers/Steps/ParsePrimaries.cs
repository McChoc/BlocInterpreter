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
            [KeywordToken(Keyword.GLOBAL), SymbolToken(Symbol.ACCESS_MEMBER), INamedIdentifierToken token] => new GlobalOperator(_nextStep.Parse(new() { token })),
            [KeywordToken(Keyword.MODULE), SymbolToken(Symbol.ACCESS_MEMBER), INamedIdentifierToken token] => new ModuleOperator(_nextStep.Parse(new() { token })),
            [KeywordToken(Keyword.CLOSURE), SymbolToken(Symbol.ACCESS_MEMBER), INamedIdentifierToken token] => new ClosureOperator(_nextStep.Parse(new() { token })),
            [KeywordToken(Keyword.PARAMS), SymbolToken(Symbol.ACCESS_MEMBER), INamedIdentifierToken token] => new ParamsOperator(_nextStep.Parse(new() { token })),
            [KeywordToken(Keyword.LOCAL), SymbolToken(Symbol.ACCESS_MEMBER), INamedIdentifierToken token] => new LocalOperator(_nextStep.Parse(new() { token })),
            [.., _, SymbolToken(Symbol.ACCESS_MEMBER), INamedIdentifierToken token] => new MemberAccessOperator(Parse(tokens.GetRange(..^2)), token.GetIdentifier()),
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
                    [SymbolToken(Symbol.UNPACK_ITER), ..] => new IndexerOperator.Argument(ExpressionParser.Parse(part.GetRange(1..)), true),
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
                    [SymbolToken(Symbol.UNPACK_ITER), ..] => new InvocationOperator.Argument(null, ExpressionParser.Parse(part.GetRange(1..)), ArgumentType.UnpackedArray),
                    [SymbolToken(Symbol.UNPACK_STRUCT), ..] => new InvocationOperator.Argument(null, ExpressionParser.Parse(part.GetRange(1..)), ArgumentType.UnpackedStruct),
                    [INamedIdentifierToken token, SymbolToken(Symbol.ASSIGN), _, ..] => new InvocationOperator.Argument(token.GetIdentifier(), ExpressionParser.Parse(part.GetRange(2..)), ArgumentType.Keyword),
                    _ => new InvocationOperator.Argument(null, ExpressionParser.Parse(part), ArgumentType.Positional)
                };

                arguments.Add(argument);
            }
        }

        return arguments;
    }
}