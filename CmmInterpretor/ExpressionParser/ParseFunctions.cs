using CmmInterpretor.Utils.Exceptions;
using CmmInterpretor.Expressions;
using CmmInterpretor.Extensions;
using CmmInterpretor.Statements;
using CmmInterpretor.Tokens;
using System.Collections.Generic;
using System.Linq;
using CmmInterpretor.Scanners;

namespace CmmInterpretor
{
    public static partial class ExpressionParser
    {
        private static IExpression ParseFunctions(List<Token> tokens, int precedence)
        {
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i] is { type: TokenType.Operator, value: "=>" })
                {
                    if (i == 0)
                        throw new SyntaxError("Missing identifiers");

                    List<string> names;

                    if (tokens[i - 1].type == TokenType.Identifier)
                    {
                        names = new()
                        {
                            tokens[i - 1].Text
                        };
                    }
                    else if (tokens[i - 1].type == TokenType.Parentheses)
                    {
                        var parameters = (List<Token>)tokens[i - 1].value;

                        names = parameters.Count > 0 ? parameters.Split(Token.Comma).Select(x => x.Single().Text).ToList() : new List<string>();

                        if (names.Count != names.Distinct().Count())
                            throw new SyntaxError("Some parameters are duplicates.");
                    }
                    else
                    {
                        throw new SyntaxError("Unexpected symbol");
                    }

                    var statement = new ReturnStatement(Parse(tokens.GetRange((i + 1)..)));

                    var function = new FunctionLiteral(false, names, new() { statement });

                    var expression = tokens.GetRange(..(i - 1));
                    expression.Add(new Token(TokenType.Literal, function));

                    return Parse(expression, precedence - 1);
                }
                else if (i < tokens.Count - 1 && tokens[i].type == TokenType.Parentheses && tokens[i + 1].type == TokenType.Block)
                {
                    var parameters = (List<Token>)tokens[i].value;

                    var names = parameters.Count > 0 ? parameters.Split(Token.Comma).Select(x => x.Single().Text).ToList() : new List<string>();

                    if (names.Count != names.Distinct().Count())
                        throw new SyntaxError("Some parameters are duplicates.");

                    var statements = StatementScanner.GetStatements(tokens[i + 1].Text);

                    var function = new FunctionLiteral(false, names, statements);

                    var expression = tokens;
                    expression.RemoveRange(i, 2);
                    expression.Insert(i, new Token(TokenType.Literal, function));

                    return Parse(expression, precedence - 1);
                }
            }

            return Parse(tokens, precedence - 1);
        }
    }
}
