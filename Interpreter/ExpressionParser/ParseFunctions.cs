using System.Collections.Generic;
using System.Linq;
using Bloc.Exceptions;
using Bloc.Expressions;
using Bloc.Extensions;
using Bloc.Scanners;
using Bloc.Statements;
using Bloc.Tokens;
using Bloc.Values;

namespace Bloc
{
    internal static partial class ExpressionParser
    {
        private static IExpression ParseFunctions(List<Token> tokens, int precedence)
        {
            for (var i = 0; i < tokens.Count; i++)
            {
                bool foundFunction = false;
                List<Token>? paramTokens = null;
                List<Statement>? statements = null;

                if (i > 0 && tokens[i].Type == TokenType.Braces && tokens[i - 1].Type == TokenType.Parentheses)
                {
                    foundFunction = true;

                    paramTokens = TokenScanner.Scan(tokens[i - 1]).ToList();

                    statements = StatementScanner.GetStatements(tokens[i].Text);

                    tokens.RemoveRange(i - 1, 2);
                }
                else if (tokens[i] is (TokenType.Operator, "=>") @operator)
                {
                    foundFunction = true;

                    if (i == 0)
                        throw new SyntaxError(@operator.Start, @operator.End, "Missing identifiers");

                    if (tokens[i - 1].Type == TokenType.Identifier)
                        paramTokens = new() { tokens[i - 1] };
                    else if (tokens[i - 1].Type == TokenType.Parentheses)
                        paramTokens = TokenScanner.Scan(tokens[i - 1]).ToList();
                    else
                        throw new SyntaxError(tokens[i - 1].Start, tokens[i - 1].End, "Unexpected symbol");

                    statements = new() { new ReturnStatement(Parse(tokens.GetRange((i + 1)..))) };

                    tokens.RemoveRange(i - 1, tokens.Count - i + 1);
                }

                if (foundFunction)
                {
                    var parameters = new List<FuncLiteral.Parameter>();

                    FuncLiteral.Parameter? argsContainer = null;
                    FuncLiteral.Parameter? kwargsContainer = null;

                    if (paramTokens!.Count > 0)
                    {
                        foreach (var part in paramTokens.Split(x => x is (TokenType.Operator, ",")))
                        {
                            if (part.Count == 0)
                            {
                                throw new SyntaxError(0, 0, "Unexpected symbol ','");
                            }
                            else if (part[0] is (TokenType.Operator, ".."))
                            {
                                if (argsContainer is not null)
                                    throw new SyntaxError(0, 0, "The array unpack syntax may only be used once in a function literal");

                                if (kwargsContainer is not null)
                                    throw new SyntaxError(0, 0, "The array unpack syntax must be used before the struct unpack syntax");

                                var name = Parse(part.GetRange(1..));

                                if (name is not Identifier identifier)
                                    throw new SyntaxError(0, 0, "Invalid identifier");

                                if (parameters.Any(x => x.Name == identifier.Name))
                                    throw new SyntaxError(part[0].Start, part[^1].End, "Some parameters are duplicates.");

                                argsContainer = new(identifier.Name, null);
                            }
                            else if (part[0] is (TokenType.Operator, "..."))
                            {
                                if (kwargsContainer is not null)
                                    throw new SyntaxError(0, 0, "The struct unpack syntax may only be used once in a function literal");

                                var name = Parse(part.GetRange(1..));

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

                                var index = part.FindIndex(x => x is (TokenType.Operator, "="));

                                if (index == -1)
                                {
                                    name = Parse(part);
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

                                    name = Parse(nameTokens);
                                    defaultValue = Parse(valueTokens);
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
                        if (tokens[j] is (TokenType.Keyword, "async"))
                        {
                            if (type == FunctionType.Asynchronous)
                                throw new SyntaxError(tokens[j].Start, tokens[j].End, "'async' modifier doubled");

                            if (type == FunctionType.Generator)
                            throw new SyntaxError(tokens[j].Start, tokens[j].End, "a function cannot be both async and a generator");

                            type = FunctionType.Asynchronous;
                        }
                        else if (tokens[j] is (TokenType.Keyword, "gen"))
                        {
                            if (type == FunctionType.Asynchronous)
                                throw new SyntaxError(tokens[j].Start, tokens[j].End, "a function cannot be both async and a generator");

                            if (type == FunctionType.Generator)
                                throw new SyntaxError(tokens[j].Start, tokens[j].End, "'generator' modifier doubled");

                            type = FunctionType.Generator;
                        }
                        else if (tokens[j] is (TokenType.Keyword, "val"))
                        {
                            if (mode == CaptureMode.Value)
                                throw new SyntaxError(tokens[j].Start, tokens[j].End, "'val' modifier doubled");

                            if (mode == CaptureMode.Reference)
                                throw new SyntaxError(tokens[j].Start, tokens[j].End, "multiple capture modifier");

                            mode = CaptureMode.Value;
                        }
                        else if (tokens[j] is (TokenType.Keyword, "ref"))
                        {
                            if (mode == CaptureMode.Value)
                                throw new SyntaxError(tokens[j].Start, tokens[j].End, "multiple capture modifier");

                            if (mode == CaptureMode.Reference)
                                throw new SyntaxError(tokens[j].Start, tokens[j].End, "'ref' modifier doubled");

                            mode = CaptureMode.Reference;
                        }
                        else
                        {
                            break;
                        }

                        tokens.RemoveAt(j);
                        j--;
                    }

                    var func = new FuncLiteral(type, mode, argsContainer, kwargsContainer, parameters, statements!);

                    tokens.Insert(j + 1, new Literal(0, 0, func));

                    return ParseFunctions(tokens, precedence);
                }
            }

            return Parse(tokens, precedence - 1);
        }
    }
}