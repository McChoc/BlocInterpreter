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
                var foundFunction = false;
                List<Token> parameters = null!;
                List<Statement> statements = null!;

                if (i > 0 && tokens[i].Type == TokenType.Braces && tokens[i - 1].Type == TokenType.Parentheses)
                {
                    foundFunction = true;

                    parameters = TokenScanner.Scan(tokens[i - 1]).ToList();

                    statements = StatementScanner.GetStatements(tokens[i].Text);

                    tokens.RemoveRange(i - 1, 2);
                }
                else if (tokens[i] is (TokenType.Operator, "=>") @operator)
                {
                    foundFunction = true;

                    if (i == 0)
                        throw new SyntaxError(@operator.Start, @operator.End, "Missing identifiers");

                    if (tokens[i - 1].Type == TokenType.Identifier)
                        parameters = new() { tokens[i - 1] };
                    else if (tokens[i - 1].Type == TokenType.Parentheses)
                        parameters = TokenScanner.Scan(tokens[i - 1]).ToList();
                    else
                        throw new SyntaxError(tokens[i - 1].Start, tokens[i - 1].End, "Unexpected symbol");

                    statements = new() { new ReturnStatement(Parse(tokens.GetRange((i + 1)..))) };

                    tokens.RemoveRange(i - 1, tokens.Count - i + 1);
                }

                if (foundFunction)
                {
                    var names = new List<string>();

                    if (parameters.Count > 0)
                    {
                        foreach (var part in parameters.Split(x => x is (TokenType.Operator, ",")))
                        {
                            var name = part.Single().Text;

                            if (names.Contains(name))
                                throw new SyntaxError(part[0].Start, part[^1].End, "Some parameters are duplicates.");

                            names.Add(name);
                        }
                    }

                    var async = false;
                    var mode = CaptureMode.None;

                    var j = i - 2;

                    while (j >= 0)
                    {
                        if (tokens[j] is (TokenType.Keyword, "async"))
                        {
                            if (async)
                                throw new SyntaxError(tokens[j].Start, tokens[j].End, "'async' modifier doubled");

                            async = true;
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

                    var function = new FunctionLiteral(async, mode, names, statements);

                    tokens.Insert(j + 1, new Literal(0, 0, function));

                    return ParseFunctions(tokens, precedence);
                }
            }

            return Parse(tokens, precedence - 1);
        }
    }
}