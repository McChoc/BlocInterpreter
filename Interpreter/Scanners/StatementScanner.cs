using System.Collections.Generic;
using System.Linq;
using Bloc.Exceptions;
using Bloc.Expressions;
using Bloc.Extensions;
using Bloc.Statements;
using Bloc.Tokens;

namespace Bloc.Scanners
{
    internal static class StatementScanner
    {
        internal static List<Statement> GetStatements(string text)
        {
            var scanner = new TokenScanner(text);

            var statements = new List<Statement>();

            while (scanner.HasNextToken())
                statements.Add(GetStatement(scanner));

            return statements;
        }

        private static Statement GetStatement(TokenScanner scanner)
        {
            string? label = null;

            var tokens = scanner.Peek(2);

            if (tokens.Count >= 2 &&
                tokens[0] is (TokenType.Identifier, var lbl) &&
                tokens[1] is (TokenType.Operator, "::"))
            {
                scanner.GetNextToken();
                scanner.GetNextToken();

                label = lbl;
            }

            var statement = scanner.Peek() switch
            {
                { Type: TokenType.Braces } => GetStatementBlock(scanner),
                (TokenType.Operator, ";") => GetEmptyStatement(scanner),
                (TokenType.Operator, "/") => GetCommandStatement(scanner),
                (TokenType.Keyword, "var") => GetVarStatement(scanner),
                (TokenType.Keyword, "if") => GetIfStatement(scanner),
                (TokenType.Keyword, "do") => GetDoStatement(scanner),
                (TokenType.Keyword, "while") => GetWhileStatement(scanner),
                (TokenType.Keyword, "until") => GetWhileStatement(scanner),
                (TokenType.Keyword, "for") => GetForStatement(scanner),
                (TokenType.Keyword, "repeat") => GetRepeatStatement(scanner),
                (TokenType.Keyword, "loop") => GetLoopStatement(scanner),
                (TokenType.Keyword, "lock") => GetLockStatement(scanner),
                (TokenType.Keyword, "try") => GetTryStatement(scanner),
                (TokenType.Keyword, "throw") => GetThrowStatement(scanner),
                (TokenType.Keyword, "return") => GetReturnStatement(scanner),
                (TokenType.Keyword, "yield") => GetYieldStatement(scanner),
                (TokenType.Keyword, "continue") => GetContinueStatement(scanner),
                (TokenType.Keyword, "break") => GetBreakStatement(scanner),
                (TokenType.Keyword, "goto") => GetGotoStatement(scanner),
                (TokenType.Keyword, "pass") => GetPassStatement(scanner),
                _ => GetExpressionStatement(scanner)
            };

            statement.Label = label;

            return statement;
        }

        private static Statement GetStatementBlock(TokenScanner scanner)
        {
            return new StatementBlock
            {
                Statements = GetStatements(scanner.GetNextToken().Text)
            };
        }

        private static Statement GetExpressionStatement(TokenScanner scanner)
        {
            return new ExpressionStatement(ExpressionParser.Parse(GetLine(scanner)));
        }

        private static Statement GetEmptyStatement(TokenScanner scanner)
        {
            scanner.GetNextToken();
            return new EmptyStatement();
        }

        private static Statement GetPassStatement(TokenScanner scanner)
        {
            var line = GetLine(scanner).GetRange(1..);

            if (line.Count != 0)
                throw new SyntaxError(line[0].Start, line[^1].End, "unexpected token");

            return new EmptyStatement();
        }

        private static Statement GetVarStatement(TokenScanner scanner)
        {
            var statement = new VarStatement();

            var keyword = scanner.GetNextToken();

            var definitions = GetLine(scanner).Split(x => x is (TokenType.Operator, ","));

            foreach (var definition in definitions)
            {
                if (definition.Count == 0)
                    throw new SyntaxError(keyword.Start, keyword.End, "Missing identifier");

                var parts = definition.Split(x => x is (TokenType.Operator, "="));

                var identifier = ExpressionParser.Parse(parts[0]);

                var value = parts.Count > 1
                    ? ExpressionParser.Parse(definition.GetRange((parts[0].Count + 1)..))
                    : null;

                statement.Add(identifier, value);
            }

            return statement;
        }

        private static Statement GetIfStatement(TokenScanner scanner)
        {
            var @if = scanner.GetNextToken();

            var statement = new IfStatement
            {
                Condition = GetExpression(@if, scanner),
                If = GetBody(@if, scanner)
            };

            if (scanner.HasNextToken() && scanner.Peek() is (TokenType.Keyword, "else"))
            {
                var @else = scanner.GetNextToken();
                statement.Else = GetBody(@else, scanner);
            }

            return statement;
        }

        private static Statement GetWhileStatement(TokenScanner scanner)
        {
            var keyword = scanner.GetNextToken();

            return new WhileStatement
            {
                Until = keyword.Text == "until",
                Condition = GetExpression(keyword, scanner),
                Statements = GetBody(keyword, scanner)
            };
        }

        private static Statement GetDoStatement(TokenScanner scanner)
        {
            var @do = scanner.GetNextToken();

            var statement = new WhileStatement
            {
                Do = true,
                Statements = GetBody(@do, scanner)
            };

            // keyword
            if (!scanner.HasNextToken() ||
                scanner.GetNextToken() is not (TokenType.Keyword, "while" or "until") keyword)
                throw new SyntaxError(@do.Start, @do.End, "Missing 'while' or 'until'");

            statement.Until = keyword.Text == "until";
            statement.Condition = GetExpression(keyword, scanner);

            // semicolon
            if (!scanner.HasNextToken() || scanner.GetNextToken() is not (TokenType.Operator, ";"))
                throw new MissingSemicolonError(keyword.Start, keyword.End);

            return statement;
        }

        private static Statement GetForStatement(TokenScanner scanner)
        {
            var keyword = scanner.GetNextToken();

            if (!scanner.HasNextToken() || scanner.GetNextToken() is not { Type: TokenType.Parentheses } expression)
                throw new SyntaxError(keyword.Start, keyword.End, "Missing '('");

            var tokens = TokenScanner.Scan(expression).ToList();
            var parts = tokens.Split(x => x is (TokenType.Operator, ";"));

            if (parts.Count > 1)
            {
                if (parts.Count != 3)
                    throw new SyntaxError(expression.Start, expression.End, "Missing ')'");

                return new ForStatement(
                    initialisation: parts[0].Count > 0 ? ExpressionParser.Parse(parts[0]) : null,
                    condition:      parts[1].Count > 0 ? ExpressionParser.Parse(parts[1]) : null,
                    increment:      parts[2].Count > 0 ? ExpressionParser.Parse(parts[2]) : null)
                {
                    GetBody(keyword, scanner)
                };
            }

            if (tokens.Count < 1 || tokens[0].Type != TokenType.Identifier)
                throw new SyntaxError(tokens[0].Start, tokens[0].End, "Missing identifier");

            if (tokens.Count < 2 || tokens[1] is not (TokenType.Keyword, "in"))
                throw new SyntaxError(tokens[0].Start, tokens[0].End, "Missing 'in'");

            return new ForInStatement
            {
                Name = tokens[0].Text,
                Expression = ExpressionParser.Parse(tokens.GetRange(2..)),
                Statements = GetBody(keyword, scanner)
            };
        }

        private static Statement GetRepeatStatement(TokenScanner scanner)
        {
            var keyword = scanner.GetNextToken();

            return new RepeatStatement
            {
                Count = GetExpression(keyword, scanner),
                Statements = GetBody(keyword, scanner)
            };
        }

        private static Statement GetLoopStatement(TokenScanner scanner)
        {
            var keyword = scanner.GetNextToken();

            return new LoopStatement
            {
                Statements = GetBody(keyword, scanner)
            };
        }

        private static Statement GetLockStatement(TokenScanner scanner)
        {
            var keyword = scanner.GetNextToken();

            return new LockStatement
            {
                Expression = GetExpression(keyword, scanner),
                Statements = GetBody(keyword, scanner)
            };
        }

        private static Statement GetTryStatement(TokenScanner scanner)
        {
            var statement = new TryStatement
            {
                Try = GetBody(scanner.GetNextToken(), scanner)
            };

            // catch
            if (!scanner.HasNextToken())
                return statement;

            if (scanner.Peek() is (TokenType.Keyword, "catch"))
                statement.Catch = GetBody(scanner.GetNextToken(), scanner);

            // finally
            if (!scanner.HasNextToken())
                return statement;

            if (scanner.Peek() is (TokenType.Keyword, "finally"))
                statement.Finally = GetBody(scanner.GetNextToken(), scanner);

            return statement;
        }

        private static Statement GetReturnStatement(TokenScanner scanner)
        {
            var line = GetLine(scanner).GetRange(1..);

            if (line.Count == 0)
                return new ReturnStatement();

            return new ReturnStatement(ExpressionParser.Parse(line));
        }

        private static Statement GetYieldStatement(TokenScanner scanner)
        {
            var line = GetLine(scanner).GetRange(1..);

            if (line.Count == 0)
                throw new SyntaxError(0, 0, "Missing expression");

            return new YieldStatement(ExpressionParser.Parse(line));
        }

        private static Statement GetThrowStatement(TokenScanner scanner)
        {
            var line = GetLine(scanner).GetRange(1..);

            if (line.Count == 0)
                throw new SyntaxError(0, 0, "Missing exception");

            return new ThrowStatement(ExpressionParser.Parse(line));
        }

        private static Statement GetContinueStatement(TokenScanner scanner)
        {
            var line = GetLine(scanner).GetRange(1..);

            if (line.Count != 0)
                throw new SyntaxError(line[0].Start, line[^1].End, "Unexpected expression");

            return new ContinueStatement();
        }

        private static Statement GetBreakStatement(TokenScanner scanner)
        {
            var line = GetLine(scanner).GetRange(1..);

            if (line.Count != 0)
                throw new SyntaxError(line[0].Start, line[^1].End, "Unexpected expression");

            return new BreakStatement();
        }

        private static Statement GetGotoStatement(TokenScanner scanner)
        {
            var keyword = scanner.GetNextToken();
            var line = GetLine(scanner);

            if (line.Count == 0)
                throw new SyntaxError(keyword.Start, keyword.End, "Missing label.");

            if (line.Count > 1)
                throw new SyntaxError(line[1].Start, line[^1].End, "Unexpected symbol.");

            if (line[0].Type != TokenType.Identifier)
                throw new SyntaxError(line[0].Start, line[0].End, "Unexpected symbol.");

            return new GotoStatement(line[0].Text);
        }

        private static Statement GetCommandStatement(TokenScanner scanner)
        {
            var slash = scanner.GetNextToken();

            var statement = new CommandStatement();
            statement.Commands.Add(new());

            while (scanner.HasNextToken())
            {
                var token = scanner.GetNextCommandToken();

                if (token is (TokenType.Operator, ";"))
                {
                    if (statement.Commands[^1].Count == 0)
                        throw new SyntaxError(token.Start, token.End, "Missing command");

                    return statement;
                }

                if (token is (TokenType.Operator, "|>"))
                    statement.Commands.Add(new());
                else
                    statement.Commands[^1].Add(token);
            }

            if (statement.Commands[^1].Count == 0)
                throw new SyntaxError(slash.Start, slash.End, "Missing command");

            throw new MissingSemicolonError(slash.Start, slash.End);
        }

        private static List<Token> GetLine(TokenScanner scanner)
        {
            var line = new List<Token>();

            while (scanner.HasNextToken())
            {
                var token = scanner.GetNextToken();

                if (token is (TokenType.Operator, ";"))
                    return line;

                line.Add(token);
            }

            throw new MissingSemicolonError(0, 0);
        }

        private static IExpression GetExpression(Token keyword, TokenScanner scanner)
        {
            if (!scanner.HasNextToken() || scanner.GetNextToken() is not { Type: TokenType.Parentheses } expression)
                throw new SyntaxError(keyword.Start, keyword.End, "Missing '('");

            return ExpressionParser.Parse(TokenScanner.Scan(expression).ToList());
        }

        private static List<Statement> GetBody(Token keyword, TokenScanner scanner)
        {
            if (!scanner.HasNextToken())
                throw new MissingSemicolonError(keyword.Start, keyword.End);

            if (scanner.Peek().Type == TokenType.Braces)
                return GetStatements(scanner.GetNextToken().Text);

            return new List<Statement> { GetStatement(scanner) };
        }
    }
}