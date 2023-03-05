using System.Collections.Generic;
using System.Linq;
using Bloc.Constants;
using Bloc.Exceptions;
using Bloc.Expressions;
using Bloc.Scanners;
using Bloc.Statements;
using Bloc.Tokens;
using Bloc.Utils.Extensions;
using Bloc.Values;

namespace Bloc.Parsers.Steps;

internal sealed class ParseFunctions : IParsingStep
{
    public IParsingStep? NextStep { get; init; }

    public ParseFunctions(IParsingStep? nextStep)
    {
        NextStep = nextStep;
    }

    public IExpression Parse(List<Token> tokens)
    {
        for (var i = 0; i < tokens.Count; i++)
        {
            if (tokens[i] is not SymbolToken(Symbol.LAMBDA) @operator)
                continue;

            if (i == 0)
                throw new SyntaxError(@operator.Start, @operator.End, "Missing identifiers");

            var paramTokens = tokens[i - 1] switch
            {
                IIdentifierToken => new() { tokens[i - 1] },
                GroupToken group => group.Tokens,
                _ => throw new SyntaxError(tokens[i - 1].Start, tokens[i - 1].End, "Unexpected symbol")
            };

            if (i >= tokens.Count - 1)
                throw new SyntaxError(@operator.Start, @operator.End, "Missing function body");

            List<Statement> statements;

            if (tokens[i + 1] is CodeBlockToken code)
            {
                statements = StatementParser.Parse(new TokenCollection(code.Tokens));
                tokens.RemoveRange(i - 1, 3);
            }
            else
            {
                statements = new() { new ReturnStatement(ExpressionParser.Parse(tokens.GetRange((i + 1)..))) };
                tokens.RemoveRange(i - 1, tokens.Count - i + 1);
            }

            var parameters = new List<FuncLiteral.Parameter>();

            FuncLiteral.Parameter? argsContainer = null;
            FuncLiteral.Parameter? kwargsContainer = null;

            if (paramTokens.Count > 0)
            {
                foreach (var part in paramTokens.Split(x => x is SymbolToken(Symbol.COMMA)))
                {
                    if (part.Count == 0)
                    {
                        throw new SyntaxError(0, 0, $"Unexpected symbol '{Symbol.COMMA}'");
                    }
                    else if (part[0] is SymbolToken(Symbol.UNPACK_ARRAY))
                    {
                        if (argsContainer is not null)
                            throw new SyntaxError(0, 0, "The array unpack syntax may only be used once in a function literal");

                        if (kwargsContainer is not null)
                            throw new SyntaxError(0, 0, "The array unpack syntax must be used before the struct unpack syntax");

                        var name = ExpressionParser.Parse(part.GetRange(1..));

                        if (name is not Identifier identifier)
                            throw new SyntaxError(0, 0, "Invalid identifier");

                        if (parameters.Any(x => x.Name == identifier.Name))
                            throw new SyntaxError(part[0].Start, part[^1].End, "Some parameters are duplicates.");

                        argsContainer = new(identifier.Name, null);
                    }
                    else if (part[0] is SymbolToken(Symbol.UNPACK_STRUCT))
                    {
                        if (kwargsContainer is not null)
                            throw new SyntaxError(0, 0, "The struct unpack syntax may only be used once in a function literal");

                        var name = ExpressionParser.Parse(part.GetRange(1..));

                        if (name is not Identifier identifier)
                            throw new SyntaxError(0, 0, "Invalid identifier");

                        if (parameters.Any(x => x.Name == identifier.Name))
                            throw new SyntaxError(part[0].Start, part[^1].End, "Some parameters are duplicates.");

                        kwargsContainer = new(identifier.Name, null);
                    }
                    else
                    {
                        if (argsContainer is not null || kwargsContainer is not null)
                            throw new SyntaxError(0, 0, "All the parameters must apear before any unpack syntax");

                        IExpression name;
                        IExpression? defaultValue;

                        var index = part.FindIndex(x => x is SymbolToken(Symbol.ASSIGN));

                        if (index == -1)
                        {
                            name = ExpressionParser.Parse(part);
                            defaultValue = null;
                        }
                        else
                        {
                            var nameTokens = part.GetRange(..index);
                            var valueTokens = part.GetRange((index + 1)..);

                            if (nameTokens.Count == 0)
                                throw new SyntaxError(0, 0, "Missing identifier");

                            if (valueTokens.Count == 0)
                                throw new SyntaxError(0, 0, "Missing value");

                            name = ExpressionParser.Parse(nameTokens);
                            defaultValue = ExpressionParser.Parse(valueTokens);
                        }

                        if (name is not Identifier identifier)
                            throw new SyntaxError(0, 0, "Invalid identifier");

                        if (parameters.Any(x => x.Name == identifier.Name))
                            throw new SyntaxError(part[0].Start, part[^1].End, "Some parameters are duplicates.");

                        parameters.Add(new(identifier.Name, defaultValue));
                    }
                }
            }

            var type = FunctionType.Synchronous;
            var mode = CaptureMode.None;

            var j = i - 2;

            while (j >= 0)
            {
                if (tokens[j] is WordToken(Keyword.ASYNC))
                {
                    if (type == FunctionType.Asynchronous)
                        throw new SyntaxError(tokens[j].Start, tokens[j].End, $"'{Keyword.ASYNC}' modifier doubled");

                    if (type == FunctionType.Generator)
                        throw new SyntaxError(tokens[j].Start, tokens[j].End, "a function cannot be both async and a generator");

                    type = FunctionType.Asynchronous;
                }
                else if (tokens[j] is WordToken(Keyword.GENERATOR))
                {
                    if (type == FunctionType.Asynchronous)
                        throw new SyntaxError(tokens[j].Start, tokens[j].End, "a function cannot be both async and a generator");

                    if (type == FunctionType.Generator)
                        throw new SyntaxError(tokens[j].Start, tokens[j].End, $"'{Keyword.GENERATOR}' modifier doubled");

                    type = FunctionType.Generator;
                }
                else if (tokens[j] is SymbolToken(Keyword.VAL))
                {
                    if (mode == CaptureMode.Value)
                        throw new SyntaxError(tokens[j].Start, tokens[j].End, $"'{Keyword.VAL}' modifier doubled");

                    if (mode == CaptureMode.Reference)
                        throw new SyntaxError(tokens[j].Start, tokens[j].End, "multiple capture modifier");

                    mode = CaptureMode.Value;
                }
                else if (tokens[j] is SymbolToken(Keyword.REF))
                {
                    if (mode == CaptureMode.Value)
                        throw new SyntaxError(tokens[j].Start, tokens[j].End, "multiple capture modifier");

                    if (mode == CaptureMode.Reference)
                        throw new SyntaxError(tokens[j].Start, tokens[j].End, $"'{Keyword.REF}' modifier doubled");

                    mode = CaptureMode.Reference;
                }
                else
                {
                    break;
                }

                tokens.RemoveAt(j);
                j--;
            }

            var func = new FuncLiteral(type, mode, argsContainer, kwargsContainer, parameters, statements);

            tokens.Insert(j + 1, new FuncToken(0, 0, func));

            return Parse(tokens);
        }

        return NextStep!.Parse(tokens);
    }
}