using System.Collections.Generic;
using System.Linq;
using Bloc.Expressions;
using Bloc.Identifiers;
using Bloc.Scanners;
using Bloc.Statements;
using Bloc.Tokens;
using Bloc.Utils.Constants;
using Bloc.Utils.Exceptions;
using Bloc.Utils.Extensions;
using Bloc.Utils.Helpers;
using Bloc.Variables;

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
        var (@unchecked, mask, scope) = GetModifiers(provider);

        var statement = provider.Peek() switch
        {
            SymbolToken(Symbol.SEMICOLON) => GetEmptyStatement(provider),

            SymbolToken(Symbol.SLASH) => GetCommandStatement(provider),
            KeywordToken(Keyword.EXEC) => GetExecStatement(provider),

            KeywordToken(Keyword.VAR) => GetVarStatement(provider, mask, scope),
            KeywordToken(Keyword.CONST) => GetConstStatement(provider, mask, scope),
            KeywordToken(Keyword.IMPORT) => GetImportStatement(provider, scope),
            KeywordToken(Keyword.EXPORT) => GetExportStatement(provider),

            KeywordToken(Keyword.IF) => GetIfStatement(provider, false),
            KeywordToken(Keyword.UNLESS) => GetIfStatement(provider, true),
            KeywordToken(Keyword.SWITCH) => GetSwitchStatement(provider),
            KeywordToken(Keyword.TRY) => GetTryStatement(provider),
            KeywordToken(Keyword.LOCK) => GetLockStatement(provider),

            KeywordToken(Keyword.DO) => GetDoWhileStatement(provider, !@unchecked),
            KeywordToken(Keyword.WHILE) => GetWhileStatement(provider, !@unchecked, false),
            KeywordToken(Keyword.UNTIL) => GetWhileStatement(provider, !@unchecked, true),
            KeywordToken(Keyword.LOOP) => GetLoopStatement(provider, !@unchecked),
            KeywordToken(Keyword.REPEAT) => GetRepeatStatement(provider, !@unchecked),
            KeywordToken(Keyword.FOR) => GetForStatement(provider, !@unchecked),
            KeywordToken(Keyword.FOREACH) => GetForeachStatement(provider, !@unchecked),

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
            IStaticIdentifierToken { Text: string label },
            SymbolToken(Symbol.COLON)])
        {
            provider.Skip(2);
            return label;
        }

        return null;
    }

    private static (bool Unchecked, bool Mask, VariableScope Scope) GetModifiers(ITokenProvider provider)
    {
        switch (provider.PeekRange(3))
        {
            case [KeywordToken(Keyword.NEW), KeywordToken(Keyword.GLOBAL), KeywordToken(Keyword.VAR or Keyword.CONST)]:
            case [KeywordToken(Keyword.GLOBAL), KeywordToken(Keyword.NEW), KeywordToken(Keyword.VAR or Keyword.CONST)]:
                provider.Skip(2);
                return (default, true, VariableScope.Global);

            case [KeywordToken(Keyword.NEW), KeywordToken(Keyword.TOPLVL), KeywordToken(Keyword.VAR or Keyword.CONST)]:
            case [KeywordToken(Keyword.TOPLVL), KeywordToken(Keyword.NEW), KeywordToken(Keyword.VAR or Keyword.CONST)]:
                provider.Skip(2);
                return (default, true, VariableScope.Module);
        }

        switch (provider.PeekRange(2))
        {
            case [KeywordToken(Keyword.GLOBAL), KeywordToken(Keyword.IMPORT)]:
                provider.Skip();
                return (default, default, VariableScope.Global);

            case [KeywordToken(Keyword.TOPLVL), KeywordToken(Keyword.IMPORT)]:
                provider.Skip();
                return (default, default, VariableScope.Module);

            case [KeywordToken(Keyword.GLOBAL), KeywordToken(Keyword.VAR or Keyword.CONST)]:
                provider.Skip();
                return (default, false, VariableScope.Global);

            case [KeywordToken(Keyword.TOPLVL), KeywordToken(Keyword.VAR or Keyword.CONST)]:
                provider.Skip();
                return (default, false, VariableScope.Module);

            case [KeywordToken(Keyword.NEW), KeywordToken(Keyword.VAR or Keyword.CONST)]:
                provider.Skip();
                return (default, true, VariableScope.Local);

            case [WordToken(Keyword.UNCHECKED), KeywordToken(Keyword.DO)]:
            case [WordToken(Keyword.UNCHECKED), KeywordToken(Keyword.WHILE)]:
            case [WordToken(Keyword.UNCHECKED), KeywordToken(Keyword.UNTIL)]:
            case [WordToken(Keyword.UNCHECKED), KeywordToken(Keyword.LOOP)]:
            case [WordToken(Keyword.UNCHECKED), KeywordToken(Keyword.REPEAT)]:
            case [WordToken(Keyword.UNCHECKED), KeywordToken(Keyword.FOR)]:
            case [WordToken(Keyword.UNCHECKED), KeywordToken(Keyword.FOREACH)]:
                provider.Skip();
                return (true, default, default);
        }

        return default;
    }

    private static StatementBlock GetStatementBlock(ITokenProvider provider)
    {
        var block = (BracesToken)provider.Next();
        var collection = new TokenCollection(block.Tokens);
        var statements = Parse(collection);

        return new StatementBlock()
        {
            Statements = statements
        };
    }

    private static ExpressionStatement GetExpressionStatement(ITokenProvider provider)
    {
        var line = GetLine(provider);
        var expression = ExpressionParser.Parse(line);

        return new ExpressionStatement(expression);
    }

    private static CommandStatement GetCommandStatement(ITokenProvider provider)
    {
        provider.Skip();

        var line = GetLine(provider);
        var command = CommandParser.Parse(line);

        return new CommandStatement(command);
    }

    private static EmptyStatement GetEmptyStatement(ITokenProvider provider)
    {
        provider.Skip();
        return new EmptyStatement();
    }

    private static ExecStatement GetExecStatement(ITokenProvider provider)
    {
        var line = GetLine(provider).GetRange(1..);

        if (line.Count == 0)
            throw new SyntaxError(0, 0, "Missing expression");

        var expression = ExpressionParser.Parse(line);

        return new ExecStatement(expression);
    }

    private static DeclarationStatement GetVarStatement(ITokenProvider provider, bool mask, VariableScope scope)
    {
        var statement = new DeclarationStatement(mask, true, scope);

        var keyword = provider.Next();
        var definitions = GetLine(provider).Split(x => x is SymbolToken(Symbol.COMMA));

        foreach (var tokens in definitions)
        {
            if (tokens.Count == 0)
                throw new SyntaxError(keyword.Start, keyword.End, "Missing identifier");

            int index = tokens.FindIndex(x => x is SymbolToken(Symbol.ASSIGN));

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

    private static DeclarationStatement GetConstStatement(ITokenProvider provider, bool mask, VariableScope scope)
    {
        var statement = new DeclarationStatement(mask, false, scope);

        var keyword = provider.Next();
        var definitions = GetLine(provider).Split(x => x is SymbolToken(Symbol.COMMA));

        foreach (var tokens in definitions)
        {
            if (tokens.Count == 0)
                throw new SyntaxError(keyword.Start, keyword.End, "Missing identifier");

            int index = tokens.FindIndex(x => x is SymbolToken(Symbol.ASSIGN));

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

    private static Statement GetImportStatement(ITokenProvider provider, VariableScope scope)
    {
        var keyword = provider.Next();
        var line = GetLine(provider);

        if (line is [SymbolToken(Symbol.TIMES), KeywordToken(Keyword.FROM), ..])
            return GetImportAllFromStatement(line, scope);
        else if (line.Any(x => x is KeywordToken(Keyword.FROM)))
            return GetImportFromStatement(line, scope);
        else 
            return GetImportStatement(line, scope);
    }

    private static ImportStatement GetImportStatement(List<IToken> line, VariableScope scope)
    {
        var paths = line.Split(x => x is SymbolToken(Symbol.COMMA));

        var statement = new ImportStatement(scope);

        foreach (var tokens in paths)
        {
            if (tokens.Count == 0)
                throw new SyntaxError(0, 0, "Missing expression");

            var expression = ExpressionParser.Parse(tokens);

            statement.ModulePathExpressions.Add(expression);
        }

        return statement;
    }

    private static ImportFromStatement GetImportFromStatement(List<IToken> line, VariableScope scope)
    {
        var index = line.FindIndex(x => x is KeywordToken(Keyword.FROM));
        var imports = line.GetRange(..index).Split(x => x is SymbolToken(Symbol.COMMA));
        var path = line.GetRange((index + 1)..);

        var statement = new ImportFromStatement(scope, ExpressionParser.Parse(path));

        foreach (var tokens in imports)
        {
            switch (tokens)
            {
                case []:
                    throw new SyntaxError(0, 0, "Missing expression");

                case [INamedIdentifierToken token]:
                    var identifier = token.GetIdentifier();
                    statement.Imports.Add(new(identifier, null));
                    break;

                case [INamedIdentifierToken nameToken, KeywordToken(Keyword.AS), INamedIdentifierToken aliasToken]:
                    var nameIdentifier = nameToken.GetIdentifier();
                    var aliasIdentifier = aliasToken.GetIdentifier();
                    statement.Imports.Add(new(nameIdentifier, aliasIdentifier));
                    break;

                default:
                    throw new SyntaxError(0, 0, "UnexpectedSymbol");
            }
        }

        return statement;
    }

    private static ImportAllFromStatement GetImportAllFromStatement(List<IToken> line, VariableScope scope)
    {
        var pathTokens = line.GetRange(2..);
        var pathExpression = ExpressionParser.Parse(pathTokens);

        return new ImportAllFromStatement(scope, pathExpression);
    }

    private static Statement GetExportStatement(ITokenProvider provider)
    {
        var keyword = provider.Next();
        var line = GetLine(provider);

        if (line is [SymbolToken(Symbol.TIMES), KeywordToken(Keyword.FROM), ..])
            return GetExportAllFromStatement(line);
        else if (line.Any(x => x is KeywordToken(Keyword.FROM)))
            return GetExportFromStatement(line);
        else
            return GetExportStatement(line);
    }

    private static ExportStatement GetExportStatement(List<IToken> line)
    {
        var exports = line.Split(x => x is SymbolToken(Symbol.COMMA));

        var statement = new ExportStatement();

        foreach (var tokens in exports)
        {
            if (tokens.Count == 0)
                throw new SyntaxError(0, 0, "Missing expression");

            if (tokens is not [.., KeywordToken(Keyword.AS), INamedIdentifierToken])
            {
                var expression = ExpressionParser.Parse(tokens);

                statement.Exports.Add(new(expression, null));
            }
            else
            {
                var expression = ExpressionParser.Parse(tokens.GetRange(..^2));
                var identifier = IdentifierParser.Parse(tokens.GetRange(^1..));

                if (identifier is not INamedIdentifier namedIdentifier)
                    throw new SyntaxError(0, 0, "An export alias must be a named identifier");

                statement.Exports.Add(new(expression, namedIdentifier));
            }
        }

        return statement;
    }

    private static ExportFromStatement GetExportFromStatement(List<IToken> line)
    {
        var index = line.FindIndex(x => x is KeywordToken(Keyword.FROM));
        var exports = line.GetRange(..index).Split(x => x is SymbolToken(Symbol.COMMA));
        var path = line.GetRange((index + 1)..);

        var statement = new ExportFromStatement(ExpressionParser.Parse(path));

        foreach (var tokens in exports)
        {
            switch (tokens)
            {
                case []:
                    throw new SyntaxError(0, 0, "Missing expression");

                case [INamedIdentifierToken token]:
                    var identifier = token.GetIdentifier();
                    statement.Exports.Add(new(identifier, null));
                    break;

                case [INamedIdentifierToken nameToken, KeywordToken(Keyword.AS), INamedIdentifierToken aliasToken]:
                    var nameIdentifier = nameToken.GetIdentifier();
                    var aliasIdentifier = aliasToken.GetIdentifier();
                    statement.Exports.Add(new(nameIdentifier, aliasIdentifier));
                    break;

                default:
                    throw new SyntaxError(0, 0, "UnexpectedSymbol");
            }
        }

        return statement;
    }

    private static ExportAllFromStatement GetExportAllFromStatement(List<IToken> line)
    {
        var pathTokens = line.GetRange(2..);
        var pathExpression = ExpressionParser.Parse(pathTokens);

        return new ExportAllFromStatement(pathExpression);
    }

    private static IfStatement GetIfStatement(ITokenProvider provider, bool reversed)
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

    private static SwitchStatement GetSwitchStatement(ITokenProvider provider)
    {
        var keyword = provider.Next();
        var expression = GetExpression(provider, keyword);

        if (!provider.HasNext() || provider.Next() is not BracesToken braces)
            throw new SyntaxError(keyword.Start, keyword.End, $"Missing '{Symbol.BRACE_L}'");

        var cases = new List<SwitchStatement.Case>();
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

                    cases.Add(new SwitchStatement.Case(caseExpression, caseStatement));
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
            Cases = cases,
            Default = @default
        };
    }

    private static WhileStatement GetWhileStatement(ITokenProvider provider, bool @checked, bool reversed)
    {
        var keyword = provider.Next();

        return new WhileStatement(@checked, reversed, false)
        {
            Expression = GetExpression(provider, keyword),
            Statement = GetStatement(provider)
        };
    }

    private static WhileStatement GetDoWhileStatement(ITokenProvider provider, bool @checked)
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

    private static ForStatement GetForStatement(ITokenProvider provider, bool @checked)
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

    private static ForeachStatement GetForeachStatement(ITokenProvider provider, bool @checked)
    {
        var keyword = provider.Next();

        if (!provider.HasNext() || provider.Next() is not ParenthesesToken expression)
            throw new SyntaxError(keyword.Start, keyword.End, $"Missing '{Symbol.PAREN_L}'");

        var tokens = expression.Tokens;

        if (tokens.Count == 0)
            throw new SyntaxError(0, 0, "Missing identifier");

        int index = tokens.FindIndex(x => x is KeywordToken(Keyword.IN));

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

    private static RepeatStatement GetRepeatStatement(ITokenProvider provider, bool @checked)
    {
        var keyword = provider.Next();

        return new RepeatStatement(@checked)
        {
            Expression = GetExpression(provider, keyword),
            Statement = GetStatement(provider)
        };
    }

    private static LoopStatement GetLoopStatement(ITokenProvider provider, bool @checked)
    {
        provider.Skip();

        return new LoopStatement(@checked)
        {
            Statement = GetStatement(provider)
        };
    }

    private static LockStatement GetLockStatement(ITokenProvider provider)
    {
        var keyword = provider.Next();

        return new LockStatement()
        {
            Expression = GetExpression(provider, keyword),
            Statement = GetStatement(provider)
        };
    }

    private static TryStatement GetTryStatement(ITokenProvider provider)
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

    private static ReturnStatement GetReturnStatement(ITokenProvider provider)
    {
        var line = GetLine(provider).GetRange(1..);

        if (line.Count == 0)
            return new ReturnStatement();

        return new ReturnStatement(ExpressionParser.Parse(line));
    }

    private static Statement GetYieldStatement(ITokenProvider provider)
    {
        var line = GetLine(provider);

        return line switch
        {
            [_, SymbolToken(Symbol.UNPACK_ITER), ..] => new YieldManyStatement(ExpressionParser.Parse(line.GetRange(2..))),
            [_, ..] => new YieldStatement(ExpressionParser.Parse(line.GetRange(1..))),
            _ => throw new SyntaxError(0, 0, "Missing expression")
        };
    }

    private static ThrowStatement GetThrowStatement(ITokenProvider provider)
    {
        var line = GetLine(provider).GetRange(1..);

        if (line.Count == 0)
            throw new SyntaxError(0, 0, "Missing exception");

        return new ThrowStatement(ExpressionParser.Parse(line));
    }

    private static ContinueStatement GetContinueStatement(ITokenProvider provider)
    {
        var line = GetLine(provider).GetRange(1..);

        if (line.Count != 0)
            throw new SyntaxError(line[0].Start, line[^1].End, "Unexpected expression");

        return new ContinueStatement();
    }

    private static BreakStatement GetBreakStatement(ITokenProvider provider)
    {
        var line = GetLine(provider).GetRange(1..);

        if (line.Count != 0)
            throw new SyntaxError(line[0].Start, line[^1].End, "Unexpected expression");

        return new BreakStatement();
    }

    private static GotoStatement GetGotoStatement(ITokenProvider provider)
    {
        var keyword = provider.Next();
        var line = GetLine(provider);

        if (line.Count == 0)
            throw new SyntaxError(keyword.Start, keyword.End, "Missing label.");

        if (line[0] is not INamedIdentifierToken token)
            throw new SyntaxError(line[0].Start, line[0].End, "Unexpected symbol.");

        if (line.Count > 1)
            throw new SyntaxError(line[1].Start, line[^1].End, "Unexpected symbol.");

        var identifier = token.GetIdentifier();

        return new GotoStatement(identifier);
    }

    private static IExpression GetExpression(ITokenProvider provider, IToken keyword)
    {
        if (!provider.HasNext() || provider.Next() is not ParenthesesToken expression)
            throw new SyntaxError(keyword.Start, keyword.End, $"Missing '{Symbol.PAREN_L}'");

        return ExpressionParser.Parse(expression.Tokens);
    }

    private static List<IToken> GetLine(ITokenProvider provider)
    {
        var line = new List<IToken>();

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