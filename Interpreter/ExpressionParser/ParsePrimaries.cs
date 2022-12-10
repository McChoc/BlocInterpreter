﻿using System.Collections.Generic;
using System.Linq;
using Bloc.Exceptions;
using Bloc.Expressions;
using Bloc.Extensions;
using Bloc.Operators;
using Bloc.Scanners;
using Bloc.Tokens;

namespace Bloc
{
    internal static partial class ExpressionParser
    {
        private static IExpression ParsePrimaries(List<Token> tokens, int precedence)
        {
            var expression = tokens[0] switch
            {
                Literal literal => literal.Expression,

                { Type: TokenType.Identifier } => new Identifier(tokens[0].Text),
                { Type: TokenType.Braces } => ParseBlock(tokens[0]),
                { Type: TokenType.Parentheses } => Parse(TokenScanner.Scan(tokens[0]).ToList()),

                _ => throw new SyntaxError(tokens[0].Start, tokens[0].End, "Unexpected symbol")
            };

            for (var i = 1; i < tokens.Count; i++)
            {
                if (tokens[i] is (TokenType.Operator, "."))
                {
                    if (tokens.Count <= i)
                        throw new SyntaxError(tokens[i].Start, tokens[i].End, "Missing identifier");

                    if (tokens[i + 1].Type != TokenType.Identifier)
                        throw new SyntaxError(tokens[i].Start, tokens[i].End, "Missing identifier");

                    expression = new MemberAccess(expression, tokens[i + 1].Text);

                    i++;
                }
                else if (tokens[i].Type == TokenType.Brackets)
                {
                    var index = Parse(TokenScanner.Scan(tokens[i]).ToList());

                    expression = new Indexer(expression, index);
                }
                else if (tokens[i].Type == TokenType.Parentheses)
                {
                    var content = TokenScanner.Scan(tokens[i]).ToList();

                    var arguments = new List<Invocation.Argument>();

                    if (content.Count > 0)
                    {
                        foreach (var part in content.Split(x => x is (TokenType.Operator, ",")))
                        {
                            if (part.Count == 0)
                            {
                                if (arguments.Count > 0 && arguments[^1].Type is Invocation.ArgumentType.Named or Invocation.ArgumentType.UnpackedStruct)
                                    throw new SyntaxError(0, 0, "All the positional arguments must apear before any named arguments");

                                arguments.Add(new(null, Invocation.ArgumentType.Positional, new VoidLiteral()));
                            }
                            else if (part[0] is (TokenType.Operator, ".."))
                            {
                                if (arguments.Count > 0 && arguments[^1].Type is Invocation.ArgumentType.Named or Invocation.ArgumentType.UnpackedStruct)
                                    throw new SyntaxError(part[0].Start, part[^1].End, "All the positional arguments must apear before any named arguments");

                                arguments.Add(new(null, Invocation.ArgumentType.UnpackedArray, Parse(part.GetRange(1..))));
                            }
                            else if (part[0] is (TokenType.Operator, "..."))
                            {
                                arguments.Add(new(null, Invocation.ArgumentType.UnpackedStruct, Parse(part.GetRange(1..))));
                            }
                            else
                            {
                                var index = part.FindIndex(x => x is (TokenType.Operator, "="));

                                if (index == -1)
                                {
                                    if (arguments.Count > 0 && arguments[^1].Type is Invocation.ArgumentType.Named or Invocation.ArgumentType.UnpackedStruct)
                                        throw new SyntaxError(part[0].Start, part[^1].End, "All the positional arguments must apear before any named arguments");

                                    arguments.Add(new(null, Invocation.ArgumentType.Positional, Parse(part)));
                                }
                                else
                                {
                                    var keyTokens = part.GetRange(..index);
                                    var valueTokens = part.GetRange((index + 1)..);

                                    if (keyTokens.Count == 0)
                                        throw new SyntaxError(0, 0, "Missing identifier");

                                    var keyExpr = Parse(keyTokens);

                                    if (keyExpr is not Identifier identifier)
                                        throw new SyntaxError(0, 0, "Invalid identifier");

                                    if (valueTokens.Count == 0)
                                        throw new SyntaxError(0, 0, "Missing value");

                                    arguments.Add(new(identifier.Name, Invocation.ArgumentType.Named, Parse(valueTokens)));
                                }
                            }
                        }
                    }

                    expression = new Invocation(expression, arguments);
                }
                else
                {
                    throw new SyntaxError(tokens[i].Start, tokens[i].End, "Unexpected symbol");
                }
            }

            return expression;
        }
    }
}