using CmmInterpretor.Exceptions;
using CmmInterpretor.Extensions;
using CmmInterpretor.Tokens;
using System;
using System.Collections.Generic;

namespace CmmInterpretor.Statements
{
    public class StatementScanner
    {
        private readonly TokenScanner _scanner;

        public StatementScanner(TokenScanner scanner) => _scanner = scanner;

        public bool HasNextStatement() => _scanner.HasNextToken();

        public Statement GetNextStatement()
        {
            if (!_scanner.HasNextToken())
                throw new Exception();

            Token firstToken = _scanner.Peek();

            if (firstToken.type == TokenType.Block)
                return GetBlockStatement();

            if (firstToken is { type : TokenType.Operator, Text : ";" })
                return GetEmptyStatement();

            if (firstToken is { type : TokenType.Operator, Text : "::" })
                return GetLabelStatement();

            if (firstToken is { type: TokenType.Operator, Text: "@" })
                return GetDefStatement();

            if (firstToken is { type: TokenType.Operator, Text: "@@" })
                throw new NotImplementedException();

            if (firstToken is { type: TokenType.Operator, Text: "/" })
                return GetCommandStatement();

            if (firstToken.type == TokenType.Keyword)
                return firstToken.value switch
                {
                    "def" => GetDefStatement(),
                    "undef" => GetUndefStatement(),

                    "if" => GetIfStatement(),

                    "do" or
                    "while" or
                    "until" => GetWhileStatement(),

                    "loop" => GetLoopStatement(),
                    "repeat" => GetRepeatStatement(),
                    "for" => GetForStatement(),

                    "try" => GetTryStatement(),

                    "return" => GetReturnStatement(),
                    "exit" => GetExitStatement(),
                    "throw" => GetThrowStatement(),
                    "continue" => GetContinueStatement(),
                    "break" => GetBreakStatement(),
                    "goto" => GetGotoStatement(),

                    "pass" => GetPassStatement(),

                    _ => new ExpressionStatement(GetLine())
                };

            return new ExpressionStatement(GetLine());
        }

        private List<Token> GetLine()
        {
            var line = new List<Token>();

            while (_scanner.HasNextToken())
            {
                Token token = _scanner.GetNextToken();

                if (token is { type : TokenType.Operator, Text : ";" })
                    return line;

                line.Add(token);
            }

            throw new SyntaxError("missing semicolon");
        }

        private Statement GetBlockStatement()
        {
            return new ScopeStatement
            {
                body = ParseCodeBlock(_scanner.GetNextToken().Text)
            };
        }

        private Statement GetEmptyStatement()
        {
            _scanner.GetNextToken();
            return new EmptyStatement();
        }

        private Statement GetDefStatement()
        {
            var statement = new DefStatement();

        //Decorator:
        //    Token token = _scanner.Peek();
        //    if (token.type == TokenType.Operator && token.Text == "@")
        //    {
        //        _scanner.GetNextToken();
        //        DefStatement.Decorator decorator = new DefStatement.Decorator();

        //        Token identifier = _scanner.GetNextToken();
        //        if (identifier.type != TokenType.Identifier)
        //            throw new Error("");
        //        decorator.identifier = identifier.Text;

        //        Token args = _scanner.Peek();
        //        if (args.type == TokenType.Parentheses)
        //            decorator.arguments = ((List<Token>)_scanner.GetNextToken().value).Split(Token.Comma);
        //        else
        //            decorator.arguments = new List<List<Token>>();

        //        statement.decorators.Add(decorator);
        //        goto Decorator;
        //    }

            Token def = _scanner.GetNextToken();
            if (def is not { type : TokenType.Keyword, Text : "def" })
                throw new SyntaxError("");

            statement.definitions = GetLine().Split(Token.Comma);

            return statement;
        }

        private Statement GetUndefStatement()
        {
            _scanner.GetNextToken();

            var undefinitions = GetLine().Split(Token.Comma);

            return new UndefStatement(undefinitions);
        }

        private Statement GetIfStatement()
        {
            var statement = new IfStatement();

        If:
            _scanner.GetNextToken();

            // condition
            if (!_scanner.HasNextToken())
                throw new SyntaxError("");
            Token condition = _scanner.GetNextToken();
            if (condition.type != TokenType.Parentheses)
                throw new SyntaxError("");
            statement.conditions.Add(condition);

            // body
            if (!_scanner.HasNextToken())
                throw new SyntaxError("");
            Token body = _scanner.Peek();
            if (body.type == TokenType.Block)
                statement.bodies.Add(ParseCodeBlock(_scanner.GetNextToken().Text));
            else
                statement.bodies.Add(new Token(TokenType.CodeBlock, new List<Statement>() { GetNextStatement() }));

            // else
            if (!_scanner.HasNextToken())
                return statement;
            Token @else = _scanner.Peek();
            if (@else is not { type : TokenType.Keyword, Text : "else" })
                return statement;
            _scanner.GetNextToken();

            //body
            if (!_scanner.HasNextToken())
                throw new SyntaxError("");
            Token token = _scanner.Peek();
            if (token is { type : TokenType.Keyword, Text : "if" })
                goto If;
            else if (token.type == TokenType.Block)
                statement.bodies.Add(ParseCodeBlock(_scanner.GetNextToken().Text));
            else
                statement.bodies.Add(new Token(TokenType.CodeBlock, new List<Statement>() { GetNextStatement() }));

            return statement;
        }

        private Statement GetLoopStatement()
        {
            var statement = new LoopStatement();

            _scanner.GetNextToken();

            // body
            if (!_scanner.HasNextToken())
                throw new SyntaxError("");
            Token body = _scanner.Peek();
            if (body.type == TokenType.Block)
                statement.body = ParseCodeBlock(_scanner.GetNextToken().Text);
            else
                statement.body = new Token(TokenType.CodeBlock, new List<Statement>() { GetNextStatement() });

            return statement;
        }

        private Statement GetRepeatStatement()
        {
            var statement = new RepeatStatement();

            _scanner.GetNextToken();

            // count
            if (!_scanner.HasNextToken())
                throw new SyntaxError("");
            Token count = _scanner.GetNextToken();
            if (count.type != TokenType.Parentheses)
                throw new SyntaxError("");
            statement.count = count;

            // body
            if (!_scanner.HasNextToken())
                throw new SyntaxError("");
            Token body = _scanner.Peek();
            if (body.type == TokenType.Block)
                statement.body = ParseCodeBlock(_scanner.GetNextToken().Text);
            else
                statement.body = new Token(TokenType.CodeBlock, new List<Statement>() { GetNextStatement() });

            return statement;
        }

        private Statement GetWhileStatement()
        {
            var statement = new WhileStatement();

            Token token = _scanner.GetNextToken();

            if (token.Text == "do")
            {
                // do
                statement.@do = true;

                // body
                if (!_scanner.HasNextToken())
                    throw new SyntaxError("");
                Token body = _scanner.Peek();
                if (body.type == TokenType.Block)
                    statement.body = ParseCodeBlock(_scanner.GetNextToken().Text);
                else
                    statement.body = new Token(TokenType.CodeBlock, new List<Statement>() { GetNextStatement() });

                // keyword
                if (!_scanner.HasNextToken())
                    throw new SyntaxError("");
                Token keyword = _scanner.GetNextToken();
                if (keyword.type != TokenType.Keyword || !(keyword.Text is "while" or "until"))
                    throw new SyntaxError("");
                if (keyword is not { type : TokenType.Keyword, Text : "while" or "until" })
                    throw new SyntaxError("");
                statement.until = keyword.Text == "until";

                // condition
                if (!_scanner.HasNextToken())
                    throw new SyntaxError("");
                Token condition = _scanner.GetNextToken();
                if (condition.type != TokenType.Parentheses)
                    throw new SyntaxError("");
                statement.condition = condition;

                if (!_scanner.HasNextToken())
                    throw new SyntaxError("");
                Token semicolon = _scanner.GetNextToken();
                if (semicolon is not { type : TokenType.Operator, Text : ";" })
                    throw new SyntaxError("missing semicolon");
            }
            else
            {
                // keyword
                statement.until = token.Text == "until";

                // condition
                if (!_scanner.HasNextToken())
                    throw new SyntaxError("");
                Token condition = _scanner.GetNextToken();
                if (condition.type != TokenType.Parentheses)
                    throw new SyntaxError("");
                statement.condition = condition;

                // body
                if (!_scanner.HasNextToken())
                    throw new SyntaxError("");
                Token body = _scanner.Peek();
                if (body.type == TokenType.Block)
                    statement.body = ParseCodeBlock(_scanner.GetNextToken().Text);
                else
                    statement.body = new Token(TokenType.CodeBlock, new List<Statement>() { GetNextStatement() });
            }

            return statement;
        }

        private Statement GetForStatement()
        {
            //dynamic statement;

            _scanner.GetNextToken();

            if (!_scanner.HasNextToken())
                throw new SyntaxError("");
            Token token = _scanner.GetNextToken();
            if (token.type != TokenType.Parentheses)
                throw new SyntaxError("");

            var tokens = (List<Token>)token.value;

            if (tokens[0].type == TokenType.Identifier && tokens[1] is { type : TokenType.Keyword, Text : "in" })
            {
                var statement = new ForInStatement
                {
                    variableName = tokens[0].Text,
                    iterable = tokens.GetRange(2..)
                };

                // body
                if (!_scanner.HasNextToken())
                    throw new SyntaxError("");
                Token body = _scanner.Peek();
                if (body.type == TokenType.Block)
                    statement.body = ParseCodeBlock(_scanner.GetNextToken().Text);
                else
                    statement.body = new Token(TokenType.CodeBlock, new List<Statement>() { GetNextStatement() });

                return statement;
            }
            else
            {
                var parts = tokens.Split(new Token(TokenType.Operator, ";"));

                var statement = new ForStatement
                {
                    initialisation = parts[0].Split(Token.Comma),
                    condition = parts[1],
                    increment = parts[2]
                };

                // body
                if (!_scanner.HasNextToken())
                    throw new SyntaxError("");
                Token body = _scanner.Peek();
                if (body.type == TokenType.Block)
                    statement.body = ParseCodeBlock(_scanner.GetNextToken().Text);
                else
                    statement.body = new Token(TokenType.CodeBlock, new List<Statement>() { GetNextStatement() });

                return statement;
            }
        }

        private Statement GetTryStatement()
        {
            var statement = new TryStatement();

            // try
            _scanner.GetNextToken();

            if (!_scanner.HasNextToken())
                throw new SyntaxError("");
            Token mainBody = _scanner.Peek();
            if (mainBody.type == TokenType.Block)
                statement.@try = ParseCodeBlock(_scanner.GetNextToken().Text);
            else
                statement.@try = new Token(TokenType.CodeBlock, new List<Statement>() { GetNextStatement() });

            // catch
            if (!_scanner.HasNextToken())
                return statement;
            Token @catch = _scanner.Peek();
            if (@catch is { type : TokenType.Keyword, Text : "catch" })
            {
                _scanner.GetNextToken();

                if (!_scanner.HasNextToken())
                    throw new SyntaxError("");
                Token body = _scanner.Peek();
                if (body.type == TokenType.Block)
                    statement.@catch = ParseCodeBlock(_scanner.GetNextToken().Text);
                else
                    statement.@catch = new Token(TokenType.CodeBlock, new List<Statement>() { GetNextStatement() });
            }

            // finally
            if (!_scanner.HasNextToken())
                return statement;
            Token @finally = _scanner.Peek();
            if (@finally.type == TokenType.Keyword && @finally.Text == "finally")
            {
                _scanner.GetNextToken();

                if (!_scanner.HasNextToken())
                    throw new SyntaxError("");
                Token body = _scanner.Peek();
                if (body.type == TokenType.Block)
                    statement.@finally = ParseCodeBlock(_scanner.GetNextToken().Text);
                else
                    statement.@finally = new Token(TokenType.CodeBlock, new List<Statement>() { GetNextStatement() });
            }

            return statement;
        }

        private Statement GetPassStatement()
        {
            List<Token> expression = GetLine().GetRange(1..);

            if (expression.Count != 0)
                throw new SyntaxError("");

            return new EmptyStatement();
        }

        private Statement GetReturnStatement()
        {
            return new ReturnStatement(GetLine().GetRange(1..));
        }

        private Statement GetExitStatement()
        {
            return new ExitStatement(GetLine().GetRange(1..));
        }

        private Statement GetThrowStatement()
        {
            return new ThrowStatement(GetLine().GetRange(1..));
        }

        private Statement GetContinueStatement()
        {
            var expression = GetLine().GetRange(1..);

            if (expression.Count != 0)
                throw new SyntaxError("");

            return new ContinueStatement();
        }

        private Statement GetBreakStatement()
        {
            var expression = GetLine().GetRange(1..);

            if (expression.Count != 0)
                throw new SyntaxError("");

            return new BreakStatement();
        }

        private Statement GetGotoStatement()
        {
            var expression = GetLine().GetRange(1..);

            if (expression.Count != 1)
                throw new SyntaxError("Unexpected symbol.");

            if (expression[0].type != TokenType.Identifier)
                throw new SyntaxError("Unexpected symbol.");

            return new GotoStatement(expression[0].Text);
        }

        private Statement GetLabelStatement()
        {
            var expression = GetLine().GetRange(1..);

            if (expression.Count != 1)
                throw new SyntaxError("Unexpected symbol.");

            if (expression[0].type != TokenType.Identifier)
                throw new SyntaxError("Unexpected symbol.");

            return new LabelStatement(expression[0].Text);
        }

        private Statement GetCommandStatement()
        {
            _scanner.GetNextToken();

            var statement = new CommandStatement
            {
                commands = new()
                {
                    new()
                }
            };

            while (_scanner.HasNextToken())
            {
                Token token = _scanner.GetNextCommandToken();

                if (token is { type : TokenType.Operator, Text : ";" })
                {
                    if (statement.commands[^1].Count == 0)
                        throw new SyntaxError("Missing command");

                    return statement;
                }
                else if (token is { type : TokenType.Operator, Text : "|>" })
                {
                    statement.commands.Add(new());
                }
                else
                {
                    statement.commands[^1].Add(token);
                }
            }

            if (statement.commands[^1].Count == 0)
                throw new SyntaxError("Missing command");

            throw new SyntaxError("Missing semicolon");
        }

        private Token ParseCodeBlock(string text)
        {
            var scanner = new StatementScanner(new TokenScanner(text));

            var statements = new List<Statement>();

            while (scanner.HasNextStatement())
                statements.Add(scanner.GetNextStatement());

            return new Token(TokenType.CodeBlock, statements);
        }
    }
}
