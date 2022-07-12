using System.Collections.Generic;
using System.Linq;
using Bloc.Expressions;
using Bloc.Extensions;
using Bloc.Scanners;
using Bloc.Statements;
using Bloc.Tokens;
using Bloc.Utils.Exceptions;

namespace Bloc
{
    internal static partial class ExpressionParser
    {
        private static IExpression ParseFunctions(List<Token> tokens, int precedence)
        {
            for (var i = 0; i < tokens.Count; i++)
            {
                if (tokens[i] is (TokenType.Operator, "=>") @operator)
                {
                    if (i == 0)
                        throw new SyntaxError(@operator.Start, @operator.End, "Missing identifiers");

                    var async = false;

                    if (i >= 2 && tokens[i - 2] is (TokenType.Keyword, "async"))
                        async = true;

                    List<string> names;

                    if (tokens[i - 1].Type == TokenType.Identifier)
                    {
                        names = new List<string>
                        {
                            tokens[i - 1].Text
                        };
                    }
                    else if (tokens[i - 1].Type == TokenType.Parentheses)
                    {
                        var parameters = TokenScanner.Scan(tokens[i - 1]).ToList();

                        names = parameters.Count > 0
                            ? parameters.Split(x => x is (TokenType.Operator, ",")).Select(x => x.Single().Text).ToList()
                            : new List<string>();

                        if (names.Count != names.Distinct().Count())
                            throw new SyntaxError(tokens[i - 1].Start, tokens[i - 1].End, "Some parameters are duplicates.");
                    }
                    else
                    {
                        throw new SyntaxError(tokens[i - 1].Start, tokens[i - 1].End, "Unexpected symbol");
                    }

                    var statement = new ReturnStatement(Parse(tokens.GetRange((i + 1)..)));

                    var function = new FunctionLiteral(async, names, new List<Statement> { statement });

                    var expression = tokens.GetRange(..(i - (async ? 2 : 1)));
                    expression.Add(new Literal(0, 0, function));

                    return Parse(expression, precedence - 1);
                }
                else if (i < tokens.Count - 1 &&
                    tokens[i].Type == TokenType.Parentheses &&
                    tokens[i + 1].Type == TokenType.Braces)
                {
                    var parameters = TokenScanner.Scan(tokens[i]).ToList();

                    var names = parameters.Count > 0
                        ? parameters.Split(x => x is (TokenType.Operator, ",")).Select(x => x.Single().Text).ToList()
                        : new List<string>();

                    if (names.Count != names.Distinct().Count())
                        throw new SyntaxError(tokens[i].Start, tokens[i].End, "Some parameters are duplicates.");

                    var async = false;

                    if (i >= 1 && tokens[i - 1] is (TokenType.Keyword, "async"))
                        async = true;

                    var statements = StatementScanner.GetStatements(tokens[i + 1].Text);

                    var function = new FunctionLiteral(async, names, statements);

                    var expression = tokens;

                    if (async)
                    {
                        expression.RemoveRange(i - 1, 3);
                        expression.Insert(i - 1, new Literal(0, 0, function));
                    }
                    else
                    {
                        expression.RemoveRange(i, 2);
                        expression.Insert(i, new Literal(0, 0, function));
                    }

                    return Parse(expression, precedence - 1);
                }
            }

            return Parse(tokens, precedence - 1);
        }
    }
}