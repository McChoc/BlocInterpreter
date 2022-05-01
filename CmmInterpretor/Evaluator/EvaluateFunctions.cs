using CmmInterpretor.Data;
using CmmInterpretor.Exceptions;
using CmmInterpretor.Extensions;
using CmmInterpretor.Results;
using CmmInterpretor.Statements;
using CmmInterpretor.Tokens;
using CmmInterpretor.Values;
using System.Collections.Generic;
using System.Linq;

namespace CmmInterpretor
{
    public static partial class Evaluator
    {
        private static IResult EvaluateFunctions(List<Token> expr, Call call, int precedence)
        {
            for (int i = 0; i < expr.Count; i++)
            {
                if (expr[i] is { type: TokenType.Operator, value: "=>" })
                {
                    if (i == 0)
                        throw new SyntaxError("Missing identifiers");

                    List<string> names;

                    if (expr[i - 1].type == TokenType.Identifier)
                    {
                        names = new()
                        {
                            expr[i - 1].Text
                        };
                    }
                    else if (expr[i - 1].type == TokenType.Parentheses)
                    {
                        var tokens = (List<Token>)expr[i - 1].value;

                        names = tokens.Count > 0 ? tokens.Split(Token.Comma).Select(x => x.Single().Text).ToList() : new List<string>();

                        if (names.Count != names.Distinct().Count())
                            throw new SyntaxError("Some parameters are duplicates.");
                    }
                    else
                    {
                        throw new SyntaxError("Unexpected symbol");
                    }

                    var function = new Function(expr.GetRange((i + 1)..))
                    {
                        Names = names,
                        Captures = call.Capture()
                    };

                    var expression = expr.GetRange(..(i - 1));
                    expression.Add(new Token(TokenType.Literal, function));
                    return Evaluate(expression, call, precedence - 1);
                }
                else if (i < expr.Count - 1 && expr[i].type == TokenType.Parentheses && expr[i + 1].type == TokenType.Block)
                {
                    var tokens = (List<Token>)expr[i].value;

                    var names = tokens.Count > 0 ? tokens.Split(Token.Comma).Select(x => x.Single().Text).ToList() : new List<string>();

                    if (names.Count != names.Distinct().Count())
                        throw new SyntaxError("Some parameters are duplicates.");

                    var scanner = new StatementScanner(new TokenScanner(expr[i + 1].Text));

                    var statements = new List<Statement>();

                    while (scanner.HasNextStatement())
                        statements.Add(scanner.GetNextStatement());

                    var function = new Function(statements)
                    {
                        Names = names,
                        Captures = call.Capture()
                    };

                    var expression = expr;
                    expression.RemoveRange(i, 2);
                    expression.Insert(i, new Token(TokenType.Literal, function));
                    return Evaluate(expression, call, precedence - 1);
                }
            }

            return Evaluate(expr, call, precedence - 1);
        }
    }
}
