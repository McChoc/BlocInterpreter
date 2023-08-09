using System.Collections.Generic;
using System.Linq;
using Bloc.Expressions;
using Bloc.Scanners;
using Bloc.Statements;
using Bloc.Statements.Arms;
using Bloc.Tokens;
using Bloc.Utils.Constants;
using Bloc.Utils.Exceptions;
using Bloc.Utils.Extensions;
using Bloc.Utils.Helpers;

namespace Bloc.Parsers;

internal static class StatementParser
{
    internal static List<Statement> Parse(ITokenProvider provider)
    {
        var statements = new List<Statement>();

        while (provider.HasNext())
            statements.Add(GetStatement(provider));

        bool duplicate = statements
            .Where(x => x.Label is not null)
            .GroupBy(x => x.Label)
            .Any(x => x.Count() > 1);

        if (duplicate)
            throw new SyntaxError(0, 0, "Duplicate labels");

        return statements;
    }

    private static Statement GetStatement(ITokenProvider provider)
    {
        string? label = GetLabel(provider);
        bool mask = IsMasking(provider);
        bool check = IsChecked(provider);

        var statement = provider.Peek() switch
        {
            SymbolToken(Symbol.SEMICOLON) => GetEmptyStatement(provider),

            SymbolToken(Symbol.SLASH) => GetCommandStatement(provider),
            KeywordToken(Keyword.EXEC) => GetExecStatement(provider),

            KeywordToken(Keyword.VAR) => GetVarStatement(provider, mask),
            KeywordToken(Keyword.CONST) => GetConstStatement(provider, mask),

            KeywordToken(Keyword.IF) => GetIfStatement(provider, false),
            KeywordToken(Keyword.UNLESS) => GetIfStatement(provider, true),
            KeywordToken(Keyword.SWITCH) => GetSwitchStatement(provider),
            KeywordToken(Keyword.TRY) => GetTryStatement(provider),
            KeywordToken(Keyword.LOCK) => GetLockStatement(provider),

            KeywordToken(Keyword.DO) => GetDoWhileStatement(provider, check),
            KeywordToken(Keyword.WHILE) => GetWhileStatement(provider, check, false),
            KeywordToken(Keyword.UNTIL) => GetWhileStatement(provider, check, true),
            KeywordToken(Keyword.LOOP) => GetLoopStatement(provider, check),
            KeywordToken(Keyword.REPEAT) => GetRepeatStatement(provider, check),
            KeywordToken(Keyword.FOR) => GetForStatement(provider, check),
            KeywordToken(Keyword.FOREACH) => GetForeachStatement(provider, check),

            KeywordToken(Keyword.THROW) => GetThrowStatement(provider),
            KeywordToken(Keyword.RETURN) => GetReturnStatement(provider),
            KeywordToken(Keyword.YIELD) => GetYieldStatement(provider),
            KeywordToken(Keyword.CONTINUE) => GetContinueStatement(provider),
            KeywordToken(Keyword.BREAK) => GetBreakStatement(provider),
            KeywordToken(Keyword.GOTO) => GetGotoStatement(provider),

            BracesToken token when CodeBlockHelper.IsCodeBlock(token.Tokens) => GetStatementBlock(provider),

            _ => GetExpressionStatement(provider)
        };

        statement.Label = label;

        return statement;
    }

    private static string? GetLabel(ITokenProvider provider)
    {
        if (provider.PeekRange(2) is [
            IIdentifierToken { Text: var label },
            SymbolToken(Symbol.COLON)])
        {
            provider.Skip(2);
            return label;
        }

        return null;
    }

    private static bool IsMasking(ITokenProvider provider)
    {
        if (provider.PeekRange(2) is [
            KeywordToken(Keyword.NEW),
            KeywordToken(Keyword.VAR or Keyword.CONST)])
        {
            provider.Skip();
            return true;
        }

        return false;
    }

    private static bool IsChecked(ITokenProvider provider)
    {
        if (provider.PeekRange(2) is [
            WordToken(Keyword.UNCHECKED),
            KeywordToken(
                Keyword.DO or
                Keyword.WHILE or
                Keyword.UNTIL or
                Keyword.LOOP or
                Keyword.REPEAT or
                Keyword.FOR or
                Keyword.FOREACH)])
        {
            provider.Skip();
            return false;
        }

        return true;
    }

    private static Statement GetStatementBlock(ITokenProvider provider)
    {
        var block = (BracesToken)provider.Next();
        var collection = new TokenCollection(block.Tokens);
        var statements = Parse(collection);

        return new StatementBlock()
        {
            Statements = statements
        };
    }

    private static Statement GetExpressionStatement(ITokenProvider provider)
    {
        var line = GetLine(provider);
        var expression = ExpressionParser.Parse(line);

        return new ExpressionStatement(expression);
    }

    private static Statement GetCommandStatement(ITokenProvider provider)
    {
        provider.Skip();

        var line = GetLine(provider);
        var command = CommandParser.Parse(line);

        return new CommandStatement(command);
    }

    private static Statement GetEmptyStatement(ITokenProvider provider)
    {
        provider.Skip();
        return new EmptyStatement();
    }

    private static Statement GetExecStatement(ITokenProvider provider)
    {
        var line = GetLine(provider).GetRange(1..);

        if (line.Count == 0)
            throw new SyntaxError(0, 0, "Missing expression");

        var expression = ExpressionParser.Parse(line);

        return new ExecStatement(expression);
    }

    private static Statement GetVarStatement(ITokenProvider provider, bool mask)
    {
        var statement = new DeclarationStatement(mask, true);

        var keyword = provider.Next();

        var definitions = GetLine(provider).Split(x => x is SymbolToken(Symbol.COMMA));

        foreach (var tokens in definitions)
        {
            if (tokens.Count == 0)
                throw new SyntaxError(keyword.Start, keyword.End, "Missing identifier");

            var index = tokens.FindIndex(x => x is SymbolToken(Symbol.ASSIGN));

            if (index == 0)
                throw new SyntaxError(tokens[0].Start, tokens[^1].End, "Missing identifier");

            if (index == tokens.Count - 1)
                throw new SyntaxError(tokens[0].Start, tokens[^1].End, "Missing expression");

            var identifier = index != -1
                ? IdentifierParser.Parse(tokens.GetRange(..index))
                : IdentifierParser.Parse(tokens);

            var expression = index != -1
                ? ExpressionParser.Parse(tokens.GetRange((index + 1)..))
                : null;

            statement.Declarations.Add(new(identifier, expression));
        }

        return statement;
    }

    private static Statement GetConstStatement(ITokenProvider provider, bool mask)
    {
        var statement = new DeclarationStatement(mask, false);

        var keyword = provider.Next();

        var definitions = GetLine(provider).Split(x => x is SymbolToken(Symbol.COMMA));

        foreach (var tokens in definitions)
        {
            if (tokens.Count == 0)
                throw new SyntaxError(keyword.Start, keyword.End, "Missing identifier");

            var index = tokens.FindIndex(x => x is SymbolToken(Symbol.ASSIGN));

            if (index == 0)
                throw new SyntaxError(tokens[0].Start, tokens[^1].End, "Missing identifier");

            if (index == -1)
                throw new SyntaxError(tokens[0].Start, tokens[^1].End, "Missing expression");

            if (index == tokens.Count - 1)
                throw new SyntaxError(tokens[0].Start, tokens[^1].End, "Missing expression");

            var identifier = IdentifierParser.Parse(tokens.GetRange(..index));

            var expression = ExpressionParser.Parse(tokens.GetRange((index + 1)..));

            statement.Declarations.Add(new(identifier, expression));
        }

        return statement;
    }

    private static Statement GetIfStatement(ITokenProvider provider, bool reversed)
    {
        var @if = provider.Next();

        var expression = GetExpression(provider, @if);
        var then = GetStatement(provider);

        Statement? @else = null;

        if (provider.HasNext() && provider.Peek() is KeywordToken(Keyword.ELSE))
        {
            provider.Skip();
            @else = GetStatement(provider);
        }

        return new IfStatement(reversed)
        {
            Expression = expression,
            Then = then,
            Else = @else
        };
    }

    private static Statement GetSwitchStatement(ITokenProvider provider)
    {
        var keyword = provider.Next();
        var expression = GetExpression(provider, keyword);

        if (!provider.HasNext() || provider.Next() is not BracesToken braces)
            throw new SyntaxError(keyword.Start, keyword.End, $"Missing '{Symbol.BRACE_L}'");

        var arms = new List<IArm>();
        Statement? @default = null;

        var subProvider = new TokenCollection(braces.Tokens);

        while (subProvider.HasNext())
        {
            var armKeyword = subProvider.Next();

            switch (armKeyword)
            {
                case KeywordToken(Keyword.CASE):
                    var caseExpression = GetExpression(subProvider, armKeyword);
                    var caseStatement = GetStatement(subProvider);

                    arms.Add(new Case(caseExpression, caseStatement));
                    break;

                case KeywordToken(Keyword.MATCH):
                    var matchExpression = GetExpression(subProvider, armKeyword);
                    var matchStatement = GetStatement(subProvider);

                    arms.Add(new Match(matchExpression, matchStatement));
                    break;

                case KeywordToken(Keyword.DEFAULT):
                    if (@default is not null)
                        throw new SyntaxError(armKeyword.Start, armKeyword.End, "A switch statement can only have a single default statement");

                    @default = GetStatement(subProvider);
                    break;

                default:
                    throw new SyntaxError(armKeyword.Start, armKeyword.End, "Unexpected token");
            }
        }

        return new SwitchStatement()
        {
            Expression = expression,
            Arms = arms,
            Default = @default
        };
    }

    private static Statement GetWhileStatement(ITokenProvider provider, bool @checked, bool reversed)
    {
        var keyword = provider.Next();

        return new WhileStatement(@checked, reversed, false)
        {
            Expression = GetExpression(provider, keyword),
            Statement = GetStatement(provider)
        };
    }

    private static Statement GetDoWhileStatement(ITokenProvider provider, bool @checked)
    {
        var @do = provider.Next();

        var statement = GetStatement(provider);

        if (!provider.HasNext() || provider.Next() is not KeywordToken(Keyword.WHILE or Keyword.UNTIL) keyword)
            throw new SyntaxError(@do.Start, @do.End, $"Missing '{Keyword.WHILE}' or '{Keyword.UNTIL}'");

        var expression = GetExpression(provider, keyword);

        if (!provider.HasNext() || provider.Next() is not SymbolToken(Symbol.SEMICOLON))
            throw new MissingSemicolonError(keyword.Start, keyword.End);

        return new WhileStatement(@checked, keyword.Text == Keyword.UNTIL, true)
        {
            Expression = expression,
            Statement = statement
        };
    }

    private static Statement GetForStatement(ITokenProvider provider, bool @checked)
    {
        var keyword = provider.Next();

        if (!provider.HasNext() || provider.Next() is not ParenthesesToken expression)
            throw new SyntaxError(keyword.Start, keyword.End, $"Missing '{Symbol.PAREN_L}'");

        var tokens = expression.Tokens;
        var parts = tokens.Split(x => x is SymbolToken(Symbol.SEMICOLON));

        if (parts.Count != 3)
            throw new SyntaxError(expression.Start, expression.End, $"Missing '{Symbol.PAREN_R}'");

        return new ForStatement(@checked)
        {
            Initialisation = parts[0].Count > 0 ? ExpressionParser.Parse(parts[0]) : null,
            Condition = parts[1].Count > 0 ? ExpressionParser.Parse(parts[1]) : null,
            Increment = parts[2].Count > 0 ? ExpressionParser.Parse(parts[2]) : null,
            Statement = GetStatement(provider)
        };
    }

    private static Statement GetForeachStatement(ITokenProvider provider, bool @checked)
    {
        var keyword = provider.Next();

        if (!provider.HasNext() || provider.Next() is not ParenthesesToken expression)
            throw new SyntaxError(keyword.Start, keyword.End, $"Missing '{Symbol.PAREN_L}'");

        var tokens = expression.Tokens;

        if (tokens.Count == 0)
            throw new SyntaxError(0, 0, "Missing identifier");

        var index = tokens.FindIndex(x => x is KeywordToken(Keyword.IN));

        if (index == 0)
            throw new SyntaxError(tokens[0].Start, tokens[^1].End, "Missing identifier");

        if (index == -1)
            throw new SyntaxError(tokens[0].Start, tokens[^1].End, $"Missing '{Keyword.IN}' keyword");

        if (index == tokens.Count -1)
            throw new SyntaxError(tokens[0].Start, tokens[^1].End, "Missing expression");

        return new ForeachStatement(@checked)
        {
            Identifier = IdentifierParser.Parse(tokens.GetRange(..index)),
            Expression = ExpressionParser.Parse(tokens.GetRange((index + 1)..)),
            Statement = GetStatement(provider)
        };
    }

    private static Statement GetRepeatStatement(ITokenProvider provider, bool @checked)
    {
        var keyword = provider.Next();

        return new RepeatStatement(@checked)
        {
            Expression = GetExpression(provider, keyword),
            Statement = GetStatement(provider)
        };
    }

    private static Statement GetLoopStatement(ITokenProvider provider, bool @checked)
    {
        provider.Skip();

        return new LoopStatement(@checked)
        {
            Statement = GetStatement(provider)
        };
    }

    private static Statement GetLockStatement(ITokenProvider provider)
    {
        var keyword = provider.Next();

        return new LockStatement()
        {
            Expression = GetExpression(provider, keyword),
            Statement = GetStatement(provider)
        };
    }

    private static Statement GetTryStatement(ITokenProvider provider)
    {
        provider.Skip();

        var @try = GetStatement(provider);
        var catches = new List<TryStatement.Catch>();

        bool foundLastCatch = false;

        while (provider.HasNext() && provider.Peek() is KeywordToken(Keyword.CATCH))
        {
            var keyword = provider.Next();

            if (foundLastCatch)
                throw new SyntaxError(keyword.Start, keyword.End, "There cannot be another catch clause after the general catch");

            if (!provider.HasNext() || provider.Next() is not ParenthesesToken parentheses)
                throw new SyntaxError(keyword.Start, keyword.End, $"Missing '{Symbol.PAREN_L}'");

            var identifier = parentheses.Tokens.Count > 1
                ? IdentifierParser.Parse(parentheses.Tokens)
                : null;

            var expression = provider.Peek() is KeywordToken(Keyword.WHEN)
                ? GetExpression(provider, provider.Next())
                : null;

            if (expression is null)
                foundLastCatch = true;

            catches.Add(new(identifier, expression, GetStatement(provider)));
        }

        Statement? @finally = null;

        if (provider.PeekRange(1) is [KeywordToken(Keyword.FINALLY)])
        {
            provider.Skip();
            @finally = GetStatement(provider);
        }

        return new TryStatement()
        {
            Try = @try,
            Catches = catches,
            Finally = @finally
        };
    }

    private static Statement GetReturnStatement(ITokenProvider provider)
    {
        var line = GetLine(provider).GetRange(1..);

        if (line.Count == 0)
            return new ReturnStatement();

        return new ReturnStatement(ExpressionParser.Parse(line));
    }

    private static Statement GetYieldStatement(ITokenProvider provider)
    {
        var line = GetLine(provider).GetRange(1..);

        if (line.Count == 0)
            throw new SyntaxError(0, 0, "Missing expression");

        return new YieldStatement(ExpressionParser.Parse(line));
    }

    private static Statement GetThrowStatement(ITokenProvider provider)
    {
        var line = GetLine(provider).GetRange(1..);

        if (line.Count == 0)
            throw new SyntaxError(0, 0, "Missing exception");

        return new ThrowStatement(ExpressionParser.Parse(line));
    }

    private static Statement GetContinueStatement(ITokenProvider provider)
    {
        var line = GetLine(provider).GetRange(1..);

        if (line.Count != 0)
            throw new SyntaxError(line[0].Start, line[^1].End, "Unexpected expression");

        return new ContinueStatement();
    }

    private static Statement GetBreakStatement(ITokenProvider provider)
    {
        var line = GetLine(provider).GetRange(1..);

        if (line.Count != 0)
            throw new SyntaxError(line[0].Start, line[^1].End, "Unexpected expression");

        return new BreakStatement();
    }

    private static Statement GetGotoStatement(ITokenProvider provider)
    {
        var keyword = provider.Next();
        var line = GetLine(provider);

        if (line.Count == 0)
            throw new SyntaxError(keyword.Start, keyword.End, "Missing label.");

        if (line[0] is not IIdentifierToken identifier)
            throw new SyntaxError(line[0].Start, line[0].End, "Unexpected symbol.");

        if (line.Count > 1)
            throw new SyntaxError(line[1].Start, line[^1].End, "Unexpected symbol.");

        return new GotoStatement(identifier.Text);
    }

    private static IExpression GetExpression(ITokenProvider provider, Token keyword)
    {
        if (!provider.HasNext() || provider.Next() is not ParenthesesToken expression)
            throw new SyntaxError(keyword.Start, keyword.End, $"Missing '{Symbol.PAREN_L}'");

        return ExpressionParser.Parse(expression.Tokens);
    }

    private static List<Token> GetLine(ITokenProvider provider)
    {
        var line = new List<Token>();

        while (provider.HasNext())
        {
            var token = provider.Next();

            if (token is SymbolToken(Symbol.SEMICOLON))
                return line;

            line.Add(token);
        }

        throw new MissingSemicolonError(0, 0);
    }
}