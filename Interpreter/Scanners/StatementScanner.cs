using System.Collections.Generic;
using System.Linq;
using Bloc.Exceptions;
using Bloc.Expressions;
using Bloc.Extensions;
using Bloc.Statements;
using Bloc.Tokens;

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
            (TokenType.Operator, ";") => GetEmptyStatement(scanner),
            (TokenType.Operator, "/") => GetCommandStatement(scanner),
            (TokenType.Keyword, "exec") => GetExecStatement(scanner),
            (TokenType.Keyword, "var") => GetVarStatement(scanner, mask),
            (TokenType.Keyword, "const") => GetConstStatement(scanner, mask),
            (TokenType.Keyword, "if") => GetIfStatement(scanner, false),
            (TokenType.Keyword, "unless") => GetIfStatement(scanner, true),
            (TokenType.Keyword, "do") => GetDoWhileStatement(scanner, check),
            (TokenType.Keyword, "while") => GetWhileStatement(scanner, check, false),
            (TokenType.Keyword, "until") => GetWhileStatement(scanner, check, true),
            (TokenType.Keyword, "loop") => GetLoopStatement(scanner, check),
            (TokenType.Keyword, "repeat") => GetRepeatStatement(scanner, check),
            (TokenType.Keyword, "for") => GetForStatement(scanner, check),
            (TokenType.Keyword, "foreach") => GetForeachStatement(scanner, check),
            (TokenType.Keyword, "lock") => GetLockStatement(scanner),
            (TokenType.Keyword, "try") => GetTryStatement(scanner),
            (TokenType.Keyword, "throw") => GetThrowStatement(scanner),
            (TokenType.Keyword, "return") => GetReturnStatement(scanner),
            (TokenType.Keyword, "yield") => GetYieldStatement(scanner),
            (TokenType.Keyword, "continue") => GetContinueStatement(scanner),
            (TokenType.Keyword, "break") => GetBreakStatement(scanner),
            (TokenType.Keyword, "goto") => GetGotoStatement(scanner),
            _ => GetExpressionStatement(scanner)
        };

        statement.Label = label;

        return statement;
    }

    private static string? GetLabel(TokenScanner scanner)
    {
        if (scanner.Peek(2) is
        [
            (TokenType.Identifier, var label),
            (TokenType.Operator, ":")
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
            (TokenType.Keyword, "new"),
            (TokenType.Keyword, "var") or
            (TokenType.Keyword, "const")
        ])
        {
            scanner.Skip(1);
            return true;
        }

        return false;
    }

    private static bool IsChecked(TokenScanner scanner)
    {
        if (scanner.Peek(2) is
        [
            (TokenType.Keyword, "unchecked"),
            (TokenType.Keyword, "do") or
            (TokenType.Keyword, "while") or
            (TokenType.Keyword, "until") or
            (TokenType.Keyword, "loop") or
            (TokenType.Keyword, "repeat") or
            (TokenType.Keyword, "for") or
            (TokenType.Keyword, "foreach")
        ])
        {
            scanner.Skip(1);
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

            statement.Definitions.Add((identifier, value));
        }

        return statement;
    }

    private static Statement GetConstStatement(TokenScanner scanner, bool mask)
    {
        var statement = new DeclarationStatement(mask, false);

        var keyword = scanner.GetNextToken();

        var definitions = GetLine(scanner).Split(x => x is (TokenType.Operator, ","));

        foreach (var definition in definitions)
        {
            if (definition.Count == 0)
                throw new SyntaxError(keyword.Start, keyword.End, "Missing identifier");

            var parts = definition.Split(x => x is (TokenType.Operator, "="));

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

        if (scanner.HasNextToken() && scanner.Peek() is  (TokenType.Keyword, "else"))
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

        if (!scanner.HasNextToken() || scanner.GetNextToken() is not (TokenType.Keyword, "while" or "until") keyword)
            throw new SyntaxError(@do.Start, @do.End, "Missing 'while' or 'until'");

        var expression = GetExpression(keyword, scanner);

        if (!scanner.HasNextToken() || scanner.GetNextToken() is not (TokenType.Operator, ";"))
            throw new MissingSemicolonError(keyword.Start, keyword.End);

        return new WhileStatement(@checked, keyword.Text == "Until", true)
        {
            Expression = expression,
            Statement = statement
        };
    }

    private static Statement GetForStatement(TokenScanner scanner, bool @checked)
    {
        var keyword = scanner.GetNextToken();

        if (!scanner.HasNextToken() || scanner.GetNextToken() is not { Type: TokenType.Parentheses } expression)
            throw new SyntaxError(keyword.Start, keyword.End, "Missing '('");

        var tokens = TokenScanner.Scan(expression).ToList();
        var parts = tokens.Split(x => x is (TokenType.Operator, ";"));

        if (parts.Count != 3)
            throw new SyntaxError(expression.Start, expression.End, "Missing ')'");

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
            throw new SyntaxError(keyword.Start, keyword.End, "Missing '('");

        var tokens = TokenScanner.Scan(expression).ToList();

        if (tokens.Count < 1 || tokens[0].Type != TokenType.Identifier)
            throw new SyntaxError(tokens[0].Start, tokens[0].End, "Missing identifier");

        if (tokens.Count < 2 || tokens[1] is not (TokenType.Keyword, "in"))
            throw new SyntaxError(tokens[0].Start, tokens[0].End, "Missing 'in' keyword");

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

        while (scanner.HasNextToken() && scanner.Peek() is (TokenType.Keyword, "catch"))
        {
            var keyword = scanner.GetNextToken();
                
            if (foundLastCatch)
                throw new SyntaxError(keyword.Start, keyword.End, "There cannot be another catch clause after the general catch");

            if (!scanner.HasNextToken() || scanner.GetNextToken() is not { Type: TokenType.Parentheses } expression)
                throw new SyntaxError(keyword.Start, keyword.End, "Missing '('");

            var tokens = TokenScanner.Scan(expression).ToList();

            if (tokens.Count == 0)
                throw new SyntaxError(keyword.Start, keyword.End, "Missing identifier");

            if (tokens[0].Type != TokenType.Identifier)
                throw new SyntaxError(tokens[0].Start, tokens[0].End, "Unexpected symbol");

            if (tokens.Count > 1)
                throw new SyntaxError(tokens[1].Start, tokens[1].End, "Unexpected symbol");

            var identifier = tokens[0].Text;
            IExpression? when;

            if (scanner.Peek() is (TokenType.Keyword, "when"))
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

        if (scanner.Peek(1) is [(TokenType.Keyword, "finally")])
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

        if (line[0].Type != TokenType.Identifier)
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

    private static IExpression GetExpression(Token keyword, TokenScanner scanner)
    {
        if (!scanner.HasNextToken() || scanner.GetNextToken() is not { Type: TokenType.Parentheses } expression)
            throw new SyntaxError(keyword.Start, keyword.End, "Missing '('");

        return ExpressionParser.Parse(TokenScanner.Scan(expression).ToList());
    }

    private static List<Token> GetLine(TokenScanner scanner)
    {
        var line = new List<Token>();

        while (scanner.HasNextToken())
        {
            var token = scanner.GetNextToken();

            if (token is (TokenType.Operator, ":"))
                throw new SyntaxError(token.Start, token.End, "Unexpected symbol ':'");

            if (token is (TokenType.Operator, ";"))
                return line;

            line.Add(token);
        }

        throw new MissingSemicolonError(0, 0);
    }
}