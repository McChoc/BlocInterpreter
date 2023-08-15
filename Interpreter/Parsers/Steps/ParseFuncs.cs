﻿using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Expressions.Literals;
using Bloc.Funcs;
using Bloc.Scanners;
using Bloc.Statements;
using Bloc.Tokens;
using Bloc.Utils.Constants;
using Bloc.Utils.Exceptions;
using Bloc.Utils.Extensions;
using Bloc.Utils.Helpers;

namespace Bloc.Parsers.Steps;

internal sealed class ParseFuncs : ParsingStep
{
    public ParseFuncs(ParsingStep? nextStep)
        : base(nextStep) { }

    internal override IExpression Parse(List<Token> tokens)
    {
        for (int i = 0; i < tokens.Count; i++)
        {
            if (tokens[i] is not SymbolToken(Symbol.LAMBDA) @operator)
                continue;

            if (i == 0)
                throw new SyntaxError(@operator.Start, @operator.End, "Missing identifiers");

            var paramTokens = tokens[i - 1] switch
            {
                INamedIdentifierToken => new() { tokens[i - 1] },
                ParenthesesToken parentheses => parentheses.Tokens,
                _ => throw new SyntaxError(tokens[i - 1].Start, tokens[i - 1].End, "Unexpected symbol")
            };

            if (i >= tokens.Count - 1)
                throw new SyntaxError(@operator.Start, @operator.End, "Missing function body");

            List<Statement> statements;

            if (tokens[i + 1] is BracesToken braces && CodeBlockHelper.IsCodeBlock(braces.Tokens))
            {
                statements = StatementParser.Parse(new TokenCollection(braces.Tokens));
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

                        if (name is not NamedIdentifierExpression identifier)
                            throw new SyntaxError(0, 0, "Invalid identifier");

                        argsContainer = new(identifier.Identifier, null);
                    }
                    else if (part[0] is SymbolToken(Symbol.UNPACK_STRUCT))
                    {
                        if (kwargsContainer is not null)
                            throw new SyntaxError(0, 0, "The struct unpack syntax may only be used once in a function literal");

                        var name = ExpressionParser.Parse(part.GetRange(1..));

                        if (name is not NamedIdentifierExpression identifier)
                            throw new SyntaxError(0, 0, "Invalid identifier");

                        kwargsContainer = new(identifier.Identifier, null);
                    }
                    else
                    {
                        if (argsContainer is not null || kwargsContainer is not null)
                            throw new SyntaxError(0, 0, "All the parameters must apear before any unpack syntax");

                        IExpression name;
                        IExpression? defaultValue;

                        int index = part.FindIndex(x => x is SymbolToken(Symbol.ASSIGN));

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

                        if (name is not NamedIdentifierExpression identifier)
                            throw new SyntaxError(0, 0, "Invalid identifier");

                        parameters.Add(new(identifier.Identifier, defaultValue));
                    }
                }
            }

            var type = FuncType.Synchronous;
            var mode = CaptureMode.None;

            int j = i - 2;

            while (j >= 0)
            {
                if (tokens[j] is WordToken(Keyword.ASYNC))
                {
                    if (type == FuncType.Asynchronous)
                        throw new SyntaxError(tokens[j].Start, tokens[j].End, $"'{Keyword.ASYNC}' modifier doubled");

                    if (type == FuncType.Generator)
                        throw new SyntaxError(tokens[j].Start, tokens[j].End, "a function cannot be both async and a generator");

                    type = FuncType.Asynchronous;
                }
                else if (tokens[j] is WordToken(Keyword.GENERATOR))
                {
                    if (type == FuncType.Asynchronous)
                        throw new SyntaxError(tokens[j].Start, tokens[j].End, "a function cannot be both async and a generator");

                    if (type == FuncType.Generator)
                        throw new SyntaxError(tokens[j].Start, tokens[j].End, $"'{Keyword.GENERATOR}' modifier doubled");

                    type = FuncType.Generator;
                }
                else if (tokens[j] is KeywordToken(Keyword.VAL))
                {
                    if (mode == CaptureMode.Value)
                        throw new SyntaxError(tokens[j].Start, tokens[j].End, $"'{Keyword.VAL}' modifier doubled");

                    if (mode == CaptureMode.Reference)
                        throw new SyntaxError(tokens[j].Start, tokens[j].End, "multiple capture modifier");

                    mode = CaptureMode.Value;
                }
                else if (tokens[j] is KeywordToken(Keyword.REF))
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

            tokens.Insert(j + 1, new ParsedToken(0, 0, func));

            return Parse(tokens);
        }

        return NextStep!.Parse(tokens);
    }
}