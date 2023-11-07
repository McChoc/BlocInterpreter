using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Expressions.Literals;
using Bloc.Expressions.Operators;
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

    private static IExpression ParseIndexingArguments(List<IToken> tokens)
    {
        return ExpressionParser.Parse(tokens);
    }

    private static List<InvocationOperator.Argument> ParseInvocationArguments(List<IToken> tokens)
    {
        var arguments = new List<InvocationOperator.Argument>();

        if (tokens.Count > 0)
        {
            foreach (var part in tokens.Split(x => x is SymbolToken(Symbol.COMMA)))
            {
                if (part.Count == 0)
                {
                    if (arguments.Count > 0 && arguments[^1].Type is InvocationOperator.ArgumentType.Named or InvocationOperator.ArgumentType.UnpackedStruct)
                        throw new SyntaxError(0, 0, "All the positional arguments must apear before any named arguments");

                    arguments.Add(new(null, new VoidLiteral(), InvocationOperator.ArgumentType.Positional));
                }
                else if (part[0] is SymbolToken(Symbol.UNPACK_ARRAY))
                {
                    if (arguments.Count > 0 && arguments[^1].Type is InvocationOperator.ArgumentType.Named or InvocationOperator.ArgumentType.UnpackedStruct)
                        throw new SyntaxError(part[0].Start, part[^1].End, "All the positional arguments must apear before any named arguments");

                    arguments.Add(new(null, ExpressionParser.Parse(part.GetRange(1..)), InvocationOperator.ArgumentType.UnpackedArray));
                }
                else if (part[0] is SymbolToken(Symbol.UNPACK_STRUCT))
                {
                    arguments.Add(new(null, ExpressionParser.Parse(part.GetRange(1..)), InvocationOperator.ArgumentType.UnpackedStruct));
                }
                else
                {
                    int index = part.FindIndex(x => x is SymbolToken(Symbol.ASSIGN));

                    if (index == -1)
                    {
                        if (arguments.Count > 0 && arguments[^1].Type is InvocationOperator.ArgumentType.Named or InvocationOperator.ArgumentType.UnpackedStruct)
                            throw new SyntaxError(part[0].Start, part[^1].End, "All the positional arguments must apear before any named arguments");

                        arguments.Add(new(null, ExpressionParser.Parse(part), InvocationOperator.ArgumentType.Positional));
                    }
                    else
                    {
                        var keyTokens = part.GetRange(..index);
                        var valueTokens = part.GetRange((index + 1)..);

                        if (keyTokens.Count == 0)
                            throw new SyntaxError(0, 0, "Missing identifier");

                        var keyExpr = ExpressionParser.Parse(keyTokens);

                        if (keyExpr is not NamedIdentifierExpression identifier)
                            throw new SyntaxError(0, 0, "Invalid identifier");

                        if (valueTokens.Count == 0)
                            throw new SyntaxError(0, 0, "Missing value");

                        arguments.Add(new(identifier.Identifier, ExpressionParser.Parse(valueTokens), InvocationOperator.ArgumentType.Named));
                    }
                }
            }
        }

        return arguments;
    }
}