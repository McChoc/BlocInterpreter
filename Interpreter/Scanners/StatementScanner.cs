using System.Collections.Generic;
using System.Linq;
using Bloc.Exceptions;
using Bloc.Expressions;
using Bloc.Extensions;
using Bloc.Statements;
using Bloc.Tokens;
using Bloc.Utils;

namespace Bloc.Scanners;

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
        string? label = GetLabel(scanner);
        bool mask = IsMasking(scanner);
        bool check = IsChecked(scanner);

        var statement = scanner.Peek() switch
        {
            (TokenType.Braces, _) => GetStatementBlock(scanner),
            (TokenType.Symbol, Symbol.SEMICOLON) => GetEmptyStatement(scanner),
            (TokenType.Symbol, Symbol.SLASH) => GetCommandStatement(scanner),
            (TokenType.Keyword, Keyword.EXEC) => GetExecStatement(scanner),
            (TokenType.Keyword, Keyword.VAR) => GetVarStatement(scanner, mask),
            (TokenType.Keyword, Keyword.CONST) => GetConstStatement(scanner, mask),
            (TokenType.Keyword, Keyword.IF) => GetIfStatement(scanner, false),
            (TokenType.Keyword, Keyword.UNLESS) => GetIfStatement(scanner, true),
            (TokenType.Keyword, Keyword.DO) => GetDoWhileStatement(scanner, check),
            (TokenType.Keyword, Keyword.WHILE) => GetWhileStatement(scanner, check, false),
            (TokenType.Keyword, Keyword.UNTIL) => GetWhileStatement(scanner, check, true),
            (TokenType.Keyword, Keyword.LOOP) => GetLoopStatement(scanner, check),
            (TokenType.Keyword, Keyword.REPEAT) => GetRepeatStatement(scanner, check),
            (TokenType.Keyword, Keyword.FOR) => GetForStatement(scanner, check),
            (TokenType.Keyword, Keyword.FOREACH) => GetForeachStatement(scanner, check),
            (TokenType.Keyword, Keyword.LOCK) => GetLockStatement(scanner),
            (TokenType.Keyword, Keyword.TRY) => GetTryStatement(scanner),
            (TokenType.Keyword, Keyword.THROW) => GetThrowStatement(scanner),
            (TokenType.Keyword, Keyword.RETURN) => GetReturnStatement(scanner),
            (TokenType.Keyword, Keyword.YIELD) => GetYieldStatement(scanner),
            (TokenType.Keyword, Keyword.CONTINUE) => GetContinueStatement(scanner),
            (TokenType.Keyword, Keyword.BREAK) => GetBreakStatement(scanner),
            (TokenType.Keyword, Keyword.GOTO) => GetGotoStatement(scanner),
            _ => GetExpressionStatement(scanner)
        };

        statement.Label = label;

        return statement;
    }

    private static string? GetLabel(TokenScanner scanner)
    {
        if (scanner.Peek(2) is
        [
            (TokenType.Word or TokenType.Identifier, var label),
            (TokenType.Symbol, Symbol.COLON)
        ])
        {
            scanner.Skip(2);
            return label;
        }

        return null;
    }

    private static bool IsMasking(TokenScanner scanner)
    {
        if (scanner.Peek(2) is
        [
            (TokenType.Keyword, Keyword.NEW),
            (TokenType.Keyword, Keyword.VAR) or (TokenType.Keyword, Keyword.CONST)
        ])
        {
            scanner.Skip();
            return true;
        }

        return false;
    }

    private static bool IsChecked(TokenScanner scanner)
    {
        if (scanner.Peek(2) is
        [
            (TokenType.Word, Keyword.UNCHECKED),
            (TokenType.Keyword, Keyword.DO) or
            (TokenType.Keyword, Keyword.WHILE) or
            (TokenType.Keyword, Keyword.UNTIL) or
            (TokenType.Keyword, Keyword.LOOP) or
            (TokenType.Keyword, Keyword.REPEAT) or
            (TokenType.Keyword, Keyword.FOR) or
            (TokenType.Keyword, Keyword.FOREACH)
        ])
        {
            scanner.Skip();
            return false;
        }

        return true;
    }

    private static Statement GetStatementBlock(TokenScanner scanner)
    {
        var text = scanner.GetNextToken().Text;
        var statements = GetStatements(text);

        return new StatementBlock()
        {
            Statements = statements
        };
    }

    private static Statement GetExpressionStatement(TokenScanner scanner)
    {
        var line = GetLine(scanner);
        var expression = ExpressionParser.Parse(line);

        return new ExpressionStatement(expression);
    }

    private static Statement GetEmptyStatement(TokenScanner scanner)
    {
        scanner.Skip();
        return new EmptyStatement();
    }

    private static Statement GetExecStatement(TokenScanner scanner)
    {
        var line = GetLine(scanner).GetRange(1..);

        if (line.Count == 0)
            throw new SyntaxError(0, 0, "Missing expression");

        var expression = ExpressionParser.Parse(line);

        return new ExecStatement(expression);
    }

    private static Statement GetVarStatement(TokenScanner scanner, bool mask)
    {
        var statement = new DeclarationStatement(mask, true);

        var keyword = scanner.GetNextToken();

        var definitions = GetLine(scanner).Split(x => x is (TokenType.Symbol, Symbol.COMMA));

        foreach (var definition in definitions)
        {
            if (definition.Count == 0)
                throw new SyntaxError(keyword.Start, keyword.End, "Missing identifier");

            var parts = definition.Split(x => x is (TokenType.Symbol, Symbol.ASSIGN));

            var identifier = ExpressionParser.Parse(parts[0]);

            var value = parts.Count > 1
                ? ExpressionParser.Parse(definition.GetRange((parts[0].Count + 1)..))
                : null;

            statement.Definitions.Add((identifier, value));
        }

        return statement;
    }

    private static Statement GetConstStatement(TokenScanner scanner, bool mask)
    {
        var statement = new DeclarationStatement(mask, false);

        var keyword = scanner.GetNextToken();

        var definitions = GetLine(scanner).Split(x => x is (TokenType.Symbol, Symbol.COMMA));

        foreach (var definition in definitions)
        {
            if (definition.Count == 0)
                throw new SyntaxError(keyword.Start, keyword.End, "Missing identifier");

            var parts = definition.Split(x => x is (TokenType.Symbol, Symbol.ASSIGN));

            var identifier = ExpressionParser.Parse(parts[0]);

            if (parts.Count == 1)
                throw new SyntaxError(parts[0][0].Start, parts[0][^1].End, "A const needs a value");

            var value = ExpressionParser.Parse(definition.GetRange((parts[0].Count + 1)..));

            statement.Definitions.Add((identifier, value));
        }

        return statement;
    }

    private static Statement GetIfStatement(TokenScanner scanner, bool reversed)
    {
        var @if = scanner.GetNextToken();

        var expression = GetExpression(@if, scanner);
        var then = GetStatement(scanner);

        Statement? @else = null;

        if (scanner.HasNextToken() && scanner.Peek() is  (TokenType.Keyword, Keyword.ELSE))
        {
            scanner.Skip();
            @else = GetStatement(scanner);
        }

        return new IfStatement(reversed)
        {
            Expression = expression,
            Then = then,
            Else = @else
        };
    }

    private static Statement GetWhileStatement(TokenScanner scanner, bool @checked, bool reversed)
    {
        var keyword = scanner.GetNextToken();

        return new WhileStatement(@checked, reversed, false)
        {
            Expression = GetExpression(keyword, scanner),
            Statement = GetStatement(scanner)
        };
    }

    private static Statement GetDoWhileStatement(TokenScanner scanner, bool @checked)
    {
        var @do = scanner.GetNextToken();

        var statement = GetStatement(scanner);

        if (!scanner.HasNextToken() || scanner.GetNextToken() is not (TokenType.Keyword, Keyword.WHILE or Keyword.UNTIL) keyword)
            throw new SyntaxError(@do.Start, @do.End, $"Missing '{Keyword.WHILE}' or '{Keyword.UNTIL}'");

        var expression = GetExpression(keyword, scanner);

        if (!scanner.HasNextToken() || scanner.GetNextToken() is not (TokenType.Symbol, Symbol.SEMICOLON))
            throw new MissingSemicolonError(keyword.Start, keyword.End);

        return new WhileStatement(@checked, keyword.Text == Keyword.UNTIL, true)
        {
            Expression = expression,
            Statement = statement
        };
    }

    private static Statement GetForStatement(TokenScanner scanner, bool @checked)
    {
        var keyword = scanner.GetNextToken();

        if (!scanner.HasNextToken() || scanner.GetNextToken() is not { Type: TokenType.Parentheses } expression)
            throw new SyntaxError(keyword.Start, keyword.End, $"Missing '{Symbol.PAREN_L}'");

        var tokens = TokenScanner.Scan(expression).ToList();
        var parts = tokens.Split(x => x is (TokenType.Symbol, Symbol.SEMICOLON));

        if (parts.Count != 3)
            throw new SyntaxError(expression.Start, expression.End, $"Missing '{Symbol.PAREN_R}'");

        return new ForStatement(@checked)
        {
            Initialisation = parts[0].Count > 0 ? ExpressionParser.Parse(parts[0]) : null,
            Condition = parts[1].Count > 0 ? ExpressionParser.Parse(parts[1]) : null,
            Increment = parts[2].Count > 0 ? ExpressionParser.Parse(parts[2]) : null,
            Statement = GetStatement(scanner)
        };
    }

    private static Statement GetForeachStatement(TokenScanner scanner, bool @checked)
    {
        var keyword = scanner.GetNextToken();

        if (!scanner.HasNextToken() || scanner.GetNextToken() is not { Type: TokenType.Parentheses } expression)
            throw new SyntaxError(keyword.Start, keyword.End, $"Missing '{Symbol.PAREN_L}'");

        var tokens = TokenScanner.Scan(expression).ToList();

        if (tokens.Count < 1 || tokens[0].Type is not TokenType.Word or TokenType.Identifier)
            throw new SyntaxError(tokens[0].Start, tokens[0].End, "Missing identifier");

        if (tokens.Count < 2 || tokens[1] is not (TokenType.Keyword, Keyword.IN))
            throw new SyntaxError(tokens[0].Start, tokens[0].End, $"Missing '{Keyword.IN}' keyword");

        if (tokens.Count < 3)
            throw new SyntaxError(tokens[0].Start, tokens[0].End, "Missing expression");

        return new ForeachStatement(@checked)
        {
            Name = tokens[0].Text,
            Expression = ExpressionParser.Parse(tokens.GetRange(2..)),
            Statement = GetStatement(scanner)
        };
    }

    private static Statement GetRepeatStatement(TokenScanner scanner, bool @checked)
    {
        var keyword = scanner.GetNextToken();

        return new RepeatStatement(@checked)
        {
            Expression = GetExpression(keyword, scanner),
            Statement = GetStatement(scanner)
        };
    }

    private static Statement GetLoopStatement(TokenScanner scanner, bool @checked)
    {
        scanner.Skip();

        return new LoopStatement(@checked)
        {
            Statement = GetStatement(scanner)
        };
    }

    private static Statement GetLockStatement(TokenScanner scanner)
    {
        var keyword = scanner.GetNextToken();

        return new LockStatement()
        {
            Expression = GetExpression(keyword, scanner),
            Statement = GetStatement(scanner)
        };
    }

    private static Statement GetTryStatement(TokenScanner scanner)
    {
        scanner.Skip();

        var @try = GetStatement(scanner);
        var catches = new List<TryStatement.Catch>();

        bool foundLastCatch = false;

        while (scanner.HasNextToken() && scanner.Peek() is (TokenType.Keyword, Keyword.CATCH))
        {
            var keyword = scanner.GetNextToken();
                
            if (foundLastCatch)
                throw new SyntaxError(keyword.Start, keyword.End, "There cannot be another catch clause after the general catch");

            if (!scanner.HasNextToken() || scanner.GetNextToken() is not { Type: TokenType.Parentheses } expression)
                throw new SyntaxError(keyword.Start, keyword.End, $"Missing '{Symbol.PAREN_L}'");

            var tokens = TokenScanner.Scan(expression).ToList();

            if (tokens.Count == 0)
                throw new SyntaxError(keyword.Start, keyword.End, "Missing identifier");

            if (tokens[0].Type is not TokenType.Word or TokenType.Identifier)
                throw new SyntaxError(tokens[0].Start, tokens[0].End, "Unexpected symbol");

            if (tokens.Count > 1)
                throw new SyntaxError(tokens[1].Start, tokens[1].End, "Unexpected symbol");

            var identifier = tokens[0].Text;
            IExpression? when;

            if (scanner.Peek() is (TokenType.Keyword, Keyword.WHEN))
            {
                when = GetExpression(scanner.GetNextToken(), scanner);
            }
            else
            {
                when = null;
                foundLastCatch = true;
            }

            catches.Add(new(identifier, when, GetStatement(scanner)));
        }

        Statement? @finally = null;

        if (scanner.Peek(1) is [(TokenType.Keyword, Keyword.FINALLY)])
        {
            scanner.Skip();
            @finally = GetStatement(scanner);
        }

        return new TryStatement()
        {
            Try = @try,
            Catches = catches,
            Finally = @finally
        };
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

        if (line[0].Type is not TokenType.Word or TokenType.Identifier)
            throw new SyntaxError(line[0].Start, line[0].End, "Unexpected symbol.");

        if (line.Count > 1)
            throw new SyntaxError(line[1].Start, line[^1].End, "Unexpected symbol.");

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

            if (token is (TokenType.Symbol, Symbol.SEMICOLON))
            {
                if (statement.Commands[^1].Count == 0)
                    throw new SyntaxError(token.Start, token.End, "Missing command");

                return statement;
            }

            if (token is (TokenType.Symbol, Symbol.PIPE))
                statement.Commands.Add(new());
            else
                statement.Commands[^1].Add(token);
        }

        if (statement.Commands[^1].Count == 0)
            throw new SyntaxError(slash.Start, slash.End, "Missing command");

        throw new MissingSemicolonError(slash.Start, slash.End);
    }

    private static IExpression GetExpression(Token keyword, TokenScanner scanner)
    {
        if (!scanner.HasNextToken() || scanner.GetNextToken() is not { Type: TokenType.Parentheses } expression)
            throw new SyntaxError(keyword.Start, keyword.End, $"Missing '{Symbol.PAREN_L}'");

        return ExpressionParser.Parse(TokenScanner.Scan(expression).ToList());
    }

    private static List<Token> GetLine(TokenScanner scanner)
    {
        var line = new List<Token>();

        while (scanner.HasNextToken())
        {
            var token = scanner.GetNextToken();

            if (token is (TokenType.Symbol, Symbol.COLON))
                throw new SyntaxError(token.Start, token.End, $"Unexpected symbol '{Symbol.COLON}'");

            if (token is (TokenType.Symbol, Symbol.SEMICOLON))
                return line;

            line.Add(token);
        }

        throw new MissingSemicolonError(0, 0);
    }
}