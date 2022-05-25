using CmmInterpretor.Utils.Exceptions;
using CmmInterpretor.Extensions;
using CmmInterpretor.Tokens;
using System.Collections.Generic;
using CmmInterpretor.Statements;

namespace CmmInterpretor.Scanners
{
    public static class StatementScanner
    {
        public static List<Statement> GetStatements(string text)
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

            if (tokens.Count >= 2 && tokens[0].type == TokenType.Identifier && tokens[1] is { type: TokenType.Operator, value: "::" })
            {
                scanner.GetNextToken();
                scanner.GetNextToken();

                label = tokens[0].Text;
            }

            var statement = scanner.Peek() switch
            {
                { type: TokenType.Block } => GetBlockStatement(scanner),
                { type: TokenType.Operator, value: ";" } => GetEmptyStatement(scanner),
                { type: TokenType.Operator, value: "/" } => GetCommandStatement(scanner),
                { type: TokenType.Keyword, value: "def" } => GetDefStatement(scanner),
                { type: TokenType.Keyword, value: "delete" } => GetDeleteStatement(scanner),
                { type: TokenType.Keyword, value: "if" } => GetIfStatement(scanner),
                { type: TokenType.Keyword, value: "do" or "while" or "until" } => GetWhileStatement(scanner),
                { type: TokenType.Keyword, value: "loop" } => GetLoopStatement(scanner),
                { type: TokenType.Keyword, value: "repeat" } => GetRepeatStatement(scanner),
                { type: TokenType.Keyword, value: "for" } => GetForStatement(scanner),
                { type: TokenType.Keyword, value: "lock" } => GetLockStatement(scanner),
                { type: TokenType.Keyword, value: "try" } => GetTryStatement(scanner),
                { type: TokenType.Keyword, value: "throw" } => GetThrowStatement(scanner),
                { type: TokenType.Keyword, value: "return" } => GetReturnStatement(scanner),
                { type: TokenType.Keyword, value: "continue" } => GetContinueStatement(scanner),
                { type: TokenType.Keyword, value: "break" } => GetBreakStatement(scanner),
                { type: TokenType.Keyword, value: "goto" } => GetGotoStatement(scanner),
                { type: TokenType.Keyword, value: "pass" } => GetPassStatement(scanner),
                _ => GetExpressionStatement(scanner)
            };

            statement.Label = label;

            return statement;
        }

        private static Statement GetExpressionStatement(TokenScanner scanner)
        {
            return new ExpressionStatement(ExpressionParser.Parse(GetLine(scanner)));
        }

#pragma warning disable IDE0017

        private static Statement GetBlockStatement(TokenScanner scanner)
        {
            var statement = new ScopeStatement();

            statement.Statements = GetStatements(scanner.GetNextToken().Text);

            return statement;
        }

#pragma warning restore IDE0017

        private static Statement GetEmptyStatement(TokenScanner scanner)
        {
            scanner.GetNextToken();
            return new EmptyStatement();
        }

        private static Statement GetPassStatement(TokenScanner scanner)
        {
            var line = GetLine(scanner).GetRange(1..);

            if (line.Count != 0)
                throw new SyntaxError("");

            return new EmptyStatement();
        }

        private static Statement GetDefStatement(TokenScanner scanner)
        {
            var statement = new DefStatement();

            scanner.GetNextToken();

            var definitions = GetLine(scanner).Split(Token.Comma);

            foreach (var definition in definitions)
            {
                if (definition.Count == 0)
                    throw new SyntaxError("Missing identifier");

                var parts = definition.Split(new Token(TokenType.Operator, "="));

                var identifier = ExpressionParser.Parse(parts[0]);

                var value = parts.Count > 1 ? ExpressionParser.Parse(definition.GetRange((parts[0].Count + 1)..)) : null;

                statement.Definitions.Add((identifier, value));
            }

            return statement;
        }

        private static Statement GetDeleteStatement(TokenScanner scanner)
        {
            var line = GetLine(scanner).GetRange(1..);

            if (line.Count == 0)
                throw new SyntaxError("");

            return new DeleteStatement(ExpressionParser.Parse(line));
        }

        private static Statement GetIfStatement(TokenScanner scanner)
        {
            var statement = new IfStatement();

            // if
            scanner.GetNextToken();
            if (!scanner.HasNextToken())
                throw new SyntaxError("");
            Token condition = scanner.GetNextToken();
            if (condition.type != TokenType.Parentheses)
                throw new SyntaxError("");
            statement.Condition = ExpressionParser.Parse((List<Token>)condition.value);

            // body
            if (!scanner.HasNextToken())
                throw new SyntaxError("");
            Token ifBody = scanner.Peek();
            if (ifBody.type == TokenType.Block)
                statement.IfBody = GetStatements(scanner.GetNextToken().Text);
            else
                statement.IfBody = new() { GetStatement(scanner) };

            // else
            if (!scanner.HasNextToken())
                return statement;
            Token @else = scanner.Peek();
            if (@else is not { type: TokenType.Keyword, value: "else" })
                return statement;
            scanner.GetNextToken();

            // body
            if (!scanner.HasNextToken())
                throw new SyntaxError("");
            Token elseBody = scanner.Peek();
            if (elseBody.type == TokenType.Block)
                statement.ElseBody = GetStatements(scanner.GetNextToken().Text);
            else
                statement.ElseBody = new() { GetStatement(scanner) };

            return statement;
        }

        private static Statement GetLoopStatement(TokenScanner scanner)
        {
            var statement = new LoopStatement();

            scanner.GetNextToken();

            // body
            if (!scanner.HasNextToken())
                throw new SyntaxError("");
            Token body = scanner.Peek();
            if (body.type == TokenType.Block)
                statement.Statements = GetStatements(scanner.GetNextToken().Text);
            else
                statement.Statements = new() { GetStatement(scanner) };

            return statement;
        }

        private static Statement GetRepeatStatement(TokenScanner scanner)
        {
            var statement = new RepeatStatement();

            scanner.GetNextToken();

            // count
            if (!scanner.HasNextToken())
                throw new SyntaxError("");
            Token count = scanner.GetNextToken();
            if (count.type != TokenType.Parentheses)
                throw new SyntaxError("");
            statement.Count = ExpressionParser.Parse((List<Token>)count.value);

            // body
            if (!scanner.HasNextToken())
                throw new SyntaxError("");
            Token body = scanner.Peek();
            if (body.type == TokenType.Block)
                statement.Statements = GetStatements(scanner.GetNextToken().Text);
            else
                statement.Statements = new() { GetStatement(scanner) };

            return statement;
        }

        private static Statement GetWhileStatement(TokenScanner scanner)
        {
            var statement = new WhileStatement();

            Token token = scanner.GetNextToken();

            if (token.Text == "do")
            {
                // do
                statement.Do = true;

                // body
                if (!scanner.HasNextToken())
                    throw new SyntaxError("");
                Token body = scanner.Peek();
                if (body.type == TokenType.Block)
                    statement.Statements = GetStatements(scanner.GetNextToken().Text);
                else
                    statement.Statements = new() { GetStatement(scanner) };

                // keyword
                if (!scanner.HasNextToken())
                    throw new SyntaxError("");
                Token keyword = scanner.GetNextToken();
                if (keyword is not { type: TokenType.Keyword, value: "while" or "until" })
                    throw new SyntaxError("");
                statement.Until = keyword.Text == "until";

                // condition
                if (!scanner.HasNextToken())
                    throw new SyntaxError("");
                Token condition = scanner.GetNextToken();
                if (condition.type != TokenType.Parentheses)
                    throw new SyntaxError("");
                statement.Condition = ExpressionParser.Parse((List<Token>)condition.value);

                if (!scanner.HasNextToken())
                    throw new SyntaxError("");
                Token semicolon = scanner.GetNextToken();
                if (semicolon is not { type: TokenType.Operator, value: ";" })
                    throw new MissingSemicolonError();
            }
            else
            {
                // keyword
                statement.Until = token.Text == "until";

                // condition
                if (!scanner.HasNextToken())
                    throw new SyntaxError("");
                Token condition = scanner.GetNextToken();
                if (condition.type != TokenType.Parentheses)
                    throw new SyntaxError("");
                statement.Condition = ExpressionParser.Parse((List<Token>)condition.value);

                // body
                if (!scanner.HasNextToken())
                    throw new SyntaxError("");
                Token body = scanner.Peek();
                if (body.type == TokenType.Block)
                    statement.Statements = GetStatements(scanner.GetNextToken().Text);
                else
                    statement.Statements = new() { GetStatement(scanner) };
            }

            return statement;
        }

        private static Statement GetForStatement(TokenScanner scanner)
        {
            //dynamic statement;

            scanner.GetNextToken();

            if (!scanner.HasNextToken())
                throw new SyntaxError("");
            Token token = scanner.GetNextToken();
            if (token.type != TokenType.Parentheses)
                throw new SyntaxError("");

            var tokens = (List<Token>)token.value;

            if (tokens.Contains(new(TokenType.Operator, ";")))
            {
                var parts = tokens.Split(new Token(TokenType.Operator, ";"));

                var statement = new ForStatement
                {
                    Initialisation = parts[0].Count > 0 ? GetInitialisation(parts[0]) : null,
                    Condition = parts[1].Count > 0 ? ExpressionParser.Parse(parts[1]) : null,
                    Increment = parts[2].Count > 0 ? ExpressionParser.Parse(parts[2]) : null
                };

                if (!scanner.HasNextToken())
                    throw new SyntaxError("");
                Token body = scanner.Peek();
                if (body.type == TokenType.Block)
                    statement.Statements = GetStatements(scanner.GetNextToken().Text);
                else
                    statement.Statements = new() { GetStatement(scanner) };

                return statement;
            }
            else
            {
                var statement = new ForInStatement
                {
                    VariableName = tokens[0].Text,
                    Iterable = ExpressionParser.Parse(tokens.GetRange(2..))
                };

                if (!scanner.HasNextToken())
                    throw new SyntaxError("");
                Token body = scanner.Peek();
                if (body.type == TokenType.Block)
                    statement.Statements = GetStatements(scanner.GetNextToken().Text);
                else
                    statement.Statements = new() { GetStatement(scanner) };

                return statement;
            }

            static DefStatement GetInitialisation(List<Token> tokens)
            {
                var statement = new DefStatement();

                var definitions = tokens.Split(Token.Comma);

                foreach (var definition in definitions)
                {
                    if (definition.Count == 0)
                        throw new SyntaxError("Missing identifier");

                    var parts = definition.Split(new Token(TokenType.Operator, "="));

                    var identifier = ExpressionParser.Parse(parts[0]);

                    var value = parts.Count > 1 ? ExpressionParser.Parse(definition.GetRange((parts[0].Count + 1)..)) : null;

                    statement.Definitions.Add((identifier, value));
                }

                return statement;
            }
        }

        private static Statement GetLockStatement(TokenScanner scanner)
        {
            var statement = new LockStatement();

            // if
            scanner.GetNextToken();
            if (!scanner.HasNextToken())
                throw new SyntaxError("");
            Token expression = scanner.GetNextToken();
            if (expression.type != TokenType.Parentheses)
                throw new SyntaxError("");
            statement.Expression = ExpressionParser.Parse((List<Token>)expression.value);

            // body
            if (!scanner.HasNextToken())
                throw new SyntaxError("");
            Token body = scanner.Peek();
            if (body.type == TokenType.Block)
                statement.Statements = GetStatements(scanner.GetNextToken().Text);
            else
                statement.Statements = new() { GetStatement(scanner) };

            return statement;
        }

        private static Statement GetTryStatement(TokenScanner scanner)
        {
            var statement = new TryStatement();

            // try
            scanner.GetNextToken();

            if (!scanner.HasNextToken())
                throw new SyntaxError("");
            Token mainBody = scanner.Peek();
            if (mainBody.type == TokenType.Block)
                statement.Try = GetStatements(scanner.GetNextToken().Text);
            else
                statement.Try = new() { GetStatement(scanner) };

            // catch
            if (!scanner.HasNextToken())
                return statement;
            Token @catch = scanner.Peek();
            if (@catch is { type: TokenType.Keyword, value: "catch" })
            {
                scanner.GetNextToken();

                if (!scanner.HasNextToken())
                    throw new SyntaxError("");
                Token body = scanner.Peek();
                if (body.type == TokenType.Block)
                    statement.Catch = GetStatements(scanner.GetNextToken().Text);
                else
                    statement.Catch = new() { GetStatement(scanner) };
            }

            // finally
            if (!scanner.HasNextToken())
                return statement;
            Token @finally = scanner.Peek();
            if (@finally is { type: TokenType.Keyword, value: "finally" })
            {
                scanner.GetNextToken();

                if (!scanner.HasNextToken())
                    throw new SyntaxError("");
                Token body = scanner.Peek();
                if (body.type == TokenType.Block)
                    statement.Finally = GetStatements(scanner.GetNextToken().Text);
                else
                    statement.Finally = new() { GetStatement(scanner) };
            }

            return statement;
        }

        private static Statement GetReturnStatement(TokenScanner scanner)
        {
            var line = GetLine(scanner).GetRange(1..);

            if (line.Count == 0)
                return new ReturnStatement();
            else
                return new ReturnStatement(ExpressionParser.Parse(line));
        }

        private static Statement GetThrowStatement(TokenScanner scanner)
        {
            var line = GetLine(scanner).GetRange(1..);

            if (line.Count == 0)
                return new ThrowStatement();
            else
                return new ThrowStatement(ExpressionParser.Parse(line));
        }

        private static Statement GetContinueStatement(TokenScanner scanner)
        {
            var line = GetLine(scanner).GetRange(1..);

            if (line.Count != 0)
                throw new SyntaxError("");

            return new ContinueStatement();
        }

        private static Statement GetBreakStatement(TokenScanner scanner)
        {
            var line = GetLine(scanner).GetRange(1..);

            if (line.Count != 0)
                throw new SyntaxError("");

            return new ContinueStatement();
        }

        private static Statement GetGotoStatement(TokenScanner scanner)
        {
            var line = GetLine(scanner).GetRange(1..);

            if (line.Count == 0)
                throw new SyntaxError("Missing label.");

            if (line.Count > 1)
                throw new SyntaxError("Unexpected symbol.");

            if (line[0].type != TokenType.Identifier)
                throw new SyntaxError("Unexpected symbol.");

            return new GotoStatement(line[0].Text);
        }

        private static Statement GetCommandStatement(TokenScanner scanner) // TODO
        {
            scanner.GetNextToken();

            var statement = new CommandStatement();
            statement.Commands.Add(new());

            while (scanner.HasNextToken())
            {
                Token token = scanner.GetNextCommandToken();

                if (token is { type: TokenType.Operator, value: ";" })
                {
                    if (statement.Commands[^1].Count == 0)
                        throw new SyntaxError("Missing command");

                    return statement;
                }
                else if (token is { type: TokenType.Operator, value: "|>" })
                {
                    statement.Commands.Add(new());
                }
                else
                {
                    statement.Commands[^1].Add(token);
                }
            }

            if (statement.Commands[^1].Count == 0)
                throw new SyntaxError("Missing command");

            throw new MissingSemicolonError();
        }

        private static List<Token> GetLine(TokenScanner scanner)
        {
            var line = new List<Token>();

            while (scanner.HasNextToken())
            {
                Token token = scanner.GetNextToken();

                if (token is { type: TokenType.Operator, value: ";" })
                    return line;

                line.Add(token);
            }

            throw new MissingSemicolonError();
        }
    }
}
