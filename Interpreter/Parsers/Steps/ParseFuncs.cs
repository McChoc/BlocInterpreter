using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Expressions.Literals;
using Bloc.Funcs;
using Bloc.Identifiers;
using Bloc.Scanners;
using Bloc.Statements;
using Bloc.Tokens;
using Bloc.Utils.Constants;
using Bloc.Utils.Exceptions;
using Bloc.Utils.Extensions;
using Bloc.Utils.Helpers;

namespace Bloc.Parsers.Steps;

internal sealed class ParseFuncs : IParsingStep
{
    private readonly IParsingStep _nextStep;

    public ParseFuncs(IParsingStep nextStep)
    {
        _nextStep = nextStep;
    }

    public IExpression Parse(List<IToken> tokens)
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

            INamedIdentifier? packingParameterIdentifier = null;
            INamedIdentifier? kwPackingParameterIdentifier = null;

            if (paramTokens.Count > 0)
            {
                foreach (var part in paramTokens.Split(x => x is SymbolToken(Symbol.COMMA)))
                {
                    switch (part)
                    {
                        case []:
                            throw new SyntaxError(0, 0, $"Unexpected symbol '{Symbol.COMMA}'");

                        case [SymbolToken(Symbol.UNPACK_ITER), INamedIdentifierToken] when packingParameterIdentifier is not null:
                            throw new SyntaxError(0, 0, "The iterable unpack syntax may only be used once in a function literal");

                        case [SymbolToken(Symbol.UNPACK_ITER), INamedIdentifierToken token]:
                            packingParameterIdentifier = token.GetIdentifier();
                            break;

                        case [SymbolToken(Symbol.UNPACK_STRUCT), INamedIdentifierToken] when kwPackingParameterIdentifier is not null:
                            throw new SyntaxError(0, 0, "The struct unpack syntax may only be used once in a function literal");

                        case [SymbolToken(Symbol.UNPACK_STRUCT), INamedIdentifierToken token]:
                            kwPackingParameterIdentifier = token.GetIdentifier();
                            break;

                        case [INamedIdentifierToken token]:
                        {
                            var identifier = token.GetIdentifier();

                            parameters.Add(new(identifier, null, ParameterType.Standard));

                            break;
                        }

                        case [INamedIdentifierToken token, SymbolToken(Symbol.ASSIGN), _, ..]:
                        {
                            var identifier = token.GetIdentifier();
                            var defaultValueTokens = part.GetRange(2..);
                            var defaultValueExpression = ExpressionParser.Parse(defaultValueTokens);

                            parameters.Add(new(identifier, defaultValueExpression, ParameterType.Standard));
                            
                            break;
                        }

                        case [SymbolToken(Symbol.BIT_OR), INamedIdentifierToken token]:
                        {
                            var identifier = token.GetIdentifier();

                            parameters.Add(new(identifier, null, ParameterType.PositionalOnly));

                            break;
                        }

                        case [SymbolToken(Symbol.BIT_OR) , INamedIdentifierToken token, SymbolToken(Symbol.ASSIGN), _, ..]:
                        {
                            var identifier = token.GetIdentifier();
                            var defaultValueTokens = part.GetRange(3..);
                            var defaultValueExpression = ExpressionParser.Parse(defaultValueTokens);

                            parameters.Add(new(identifier, defaultValueExpression, ParameterType.PositionalOnly));
                            
                            break;
                        }

                        case [SymbolToken(Symbol.BIT_AND), INamedIdentifierToken token]:
                        {
                            var identifier = token.GetIdentifier();

                            parameters.Add(new(identifier, null, ParameterType.KeywordOnly));

                            break;
                        }

                        case [SymbolToken(Symbol.BIT_AND) , INamedIdentifierToken token, SymbolToken(Symbol.ASSIGN), _, ..]:
                        {
                            var identifier = token.GetIdentifier();
                            var defaultValueTokens = part.GetRange(3..);
                            var defaultValueExpression = ExpressionParser.Parse(defaultValueTokens);

                            parameters.Add(new(identifier, defaultValueExpression, ParameterType.KeywordOnly));
                            
                            break;
                        }

                        default:
                            throw new SyntaxError(0, 0, $"Unexpected token");
                    }
                }
            }

            var type = FuncType.Synchronous;
            var mode = CaptureMode.None;

            int j = i - 2;

            while (j >= 0)
            {
                if (tokens[j] is SymbolToken(Symbol.UNPACK_ITER))
                {
                    if (type == FuncType.Asynchronous)
                        throw new SyntaxError(tokens[j].Start, tokens[j].End, "A generator cannot be async");

                    if (type == FuncType.Generator)
                        throw new SyntaxError(tokens[j].Start, tokens[j].End, $"'{Symbol.UNPACK_ITER}' modifier doubled");

                    type = FuncType.Generator;
                }
                else if (tokens[j] is WordToken(Keyword.ASYNC))
                {
                    if (type == FuncType.Asynchronous)
                        throw new SyntaxError(tokens[j].Start, tokens[j].End, $"'{Keyword.ASYNC}' modifier doubled");

                    if (type == FuncType.Generator)
                        throw new SyntaxError(tokens[j].Start, tokens[j].End, "A generator cannot be async");

                    type = FuncType.Asynchronous;
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

            var func = new FuncLiteral(type, mode, packingParameterIdentifier, kwPackingParameterIdentifier, parameters, statements);

            tokens.Insert(j + 1, new ParsedToken(0, 0, func));

            return Parse(tokens);
        }

        return _nextStep.Parse(tokens);
    }
}