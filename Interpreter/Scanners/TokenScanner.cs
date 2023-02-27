using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Bloc.Exceptions;
using Bloc.Expressions;
using Bloc.Tokens;
using Bloc.Utils;

namespace Bloc.Scanners;

internal sealed class TokenScanner
{
    private static readonly HashSet<char> reservedCharacters = "!\"#$%&'()*+,-./:;<=>?@[\\]^`{|}~".ToHashSet();

    private readonly string _code;
    private readonly int _offset;

    private int _index;

    internal TokenScanner(string code, int offset = 0)
    {
        _offset = offset;
        _code = code;
    }

    internal static IEnumerable<Token> Scan(Token token)
    {
        return Scan(token.Text, token.Start);
    }

    private static IEnumerable<Token> Scan(string code, int offset)
    {
        var scanner = new TokenScanner(code, offset);

        while (scanner.HasNextToken())
            yield return scanner.GetNextToken();
    }

    internal bool HasNextToken()
    {
        SkipWhiteSpaceAndComments();

        return _index < _code.Length;
    }

    internal void Skip(int count = 1)
    {
        for (var i = 0; i < count && HasNextToken(); i++)
            GetNextToken();
    }

    internal Token Peek()
    {
        var previousIndex = _index;

        var token = GetNextToken();

        _index = previousIndex;

        return token;
    }

    internal List<Token> Peek(int count)
    {
        var previousIndex = _index;

        var tokens = new List<Token>();

        for (var i = 0; i < count && HasNextToken(); i++)
            tokens.Add(GetNextToken());

        _index = previousIndex;

        return tokens;
    }

    internal Token GetNextToken()
    {
        if (_index >= _code.Length)
            throw new InvalidOperationException();

        SkipWhiteSpaceAndComments();

        if (char.IsDigit(_code[_index]) || (_index < _code.Length - 1 && _code[_index] == '.' && char.IsDigit(_code[_index + 1])))
            return GetNumber();

        if (_code[_index] is '\'' or '"' or '`')
            return GetString();

        if (_code[_index] is '(' or '{' or '[')
            return GetBlock();

        if (reservedCharacters.Contains(_code[_index]))
        {
            var symbol = GetSymbol();

            if (symbol.Text != Symbol.VARIABLE)
                return symbol;

            if (_index >= _code.Length - 1)
                throw new SyntaxError(0, 0, "Missing variable name.");

            if (char.IsDigit(_code[_index]) || (_index < _code.Length - 1 && _code[_index] == '.' && char.IsDigit(_code[_index + 1])))
                throw new SyntaxError(0, 0, "Missing variable name.");

            if (_code[_index] is '\'' or '"' or '`')
                throw new SyntaxError(0, 0, "Missing variable name.");

            if (_code[_index] is '(' or '{' or '[')
                throw new SyntaxError(0, 0, "Missing variable name.");

            if (reservedCharacters.Contains(_code[_index]))
                throw new SyntaxError(0, 0, "Missing variable name.");

            return GetWord(true);
        }

        var word = GetWord(false);

        if (word is (TokenType.Keyword, Keyword.NOT) && Peek(1) is [(TokenType.Keyword, Keyword.IN)])
            return new Token(word.Start, GetNextToken().End, TokenType.Keyword, Keyword.NOT_IN);

        if (word is (TokenType.Keyword, Keyword.IS) && Peek(1) is [(TokenType.Keyword, Keyword.NOT)])
            return new Token(word.Start, GetNextToken().End, TokenType.Keyword, Keyword.IS_NOT);

        if (word is (TokenType.Keyword, Keyword.VAL) && Peek(1) is [(TokenType.Keyword, Keyword.VAL)])
            return new Token(word.Start, GetNextToken().End, TokenType.Keyword, Keyword.VAL_VAL);

        if (word is (TokenType.Keyword, Keyword.LET) && Peek(1) is [(TokenType.Keyword, Keyword.NEW)])
            return new Token(word.Start, GetNextToken().End, TokenType.Keyword, Keyword.LET_NEW);

        if (word is (TokenType.Keyword, Keyword.CONST) && Peek(1) is [(TokenType.Keyword, Keyword.NEW)])
            return new Token(word.Start, GetNextToken().End, TokenType.Keyword, Keyword.CONST_NEW);

        return word;
    }

    internal Token GetNextCommandToken()
    {
        if (_index >= _code.Length)
            throw new InvalidOperationException();

        SkipWhiteSpaceAndComments();

        if (_index < _code.Length - 1 && _code.Substring(_index, 2) == "|>")
            return new Token(_index + _offset, (_index += 2) + _offset, TokenType.Symbol, Symbol.PIPE);

        if (_code[_index] == ';')
            return new Token(_index + _offset, ++_index + _offset, TokenType.Symbol, Symbol.SEMICOLON);

        if (_code[_index] == '$')
        {
            _index++;
            return GetWord(true);
        }

        if (_code[_index] is '\'' or '"' or '`')
            return GetString();

        return GetCommand();
    }

    private void SkipWhiteSpaceAndComments()
    {
        var start = _index;
        bool skip;

        do
        {
            skip = false;

            if (_index < _code.Length - 1 && _code.Substring(_index, 2) == Symbol.COMMENT_L)
            {
                skip = true;

                _index += 2;

                while (true)
                {
                    if (_index >= _code.Length - 1)
                        throw new SyntaxError(start, start + 2, $"missing '{Symbol.COMMENT_R}'");

                    if (_code.Substring(_index, 2) == Symbol.COMMENT_R)
                    {
                        _index += 2;
                        break;
                    }

                    _index++;
                }
            }

            if (_index < _code.Length && _code[_index] == '#')
            {
                skip = true;

                _index++;

                while (true)
                {
                    if (_index == _code.Length)
                        break;

                    if (_code[_index] == '\n')
                    {
                        _index++;
                        break;
                    }

                    _index++;
                }
            }

            while (_index < _code.Length && char.IsWhiteSpace(_code[_index]))
            {
                skip = true;
                _index++;
            }
        } while (skip);
    }

    private Token GetWord(bool forceIdentifier)
    {
        var start = _index;

        while (_index < _code.Length && !char.IsWhiteSpace(_code[_index]) && !reservedCharacters.Contains(_code[_index]))
            _index++;

        var text = _code[start.._index];

        TokenType type;

        if (forceIdentifier)
            type = TokenType.Identifier;
        else if (Keyword.LiteralKeywords.Contains(text))
            type = TokenType.LiteralKeyword;
        else if (Keyword.HardKeywords.Contains(text))
            type = TokenType.Keyword;
        else
            type = TokenType.Word;

        return new Token(start + _offset, _index + _offset, type, text);
    }

    private Token GetNumber()
    {
        var hasPeriod = false;
        var hasExp = false;

        byte @base = 10;

        if (_code[_index] == '0' && _index < _code.Length - 1)
            switch (char.ToLower(_code[_index + 1]))
            {
                case 'b':
                    @base = 2;
                    _index += 2;
                    break;
                case 'o':
                    @base = 8;
                    _index += 2;
                    break;
                case 'x':
                    @base = 16;
                    _index += 2;
                    break;
            }

        var start = _index;

        while (true)
        {
            if (_index == _code.Length)
                break;

            if (_code[_index] == '.' && !hasPeriod && !hasExp && @base == 10)
            {
                _index++;
                hasPeriod = true;
                continue;
            }

            if (char.ToLower(_code[_index]) == 'e' && !hasExp && @base == 10)
            {
                _index++;
                hasExp = true;
                continue;
            }

            if ((_code[_index] == '+' || _code[_index] == '-') && _index > 0 &&
                char.ToLower(_code[_index - 1]) == 'e' && @base == 10)
            {
                _index++;
                continue;
            }

            if (char.IsLetterOrDigit(_code[_index]) || _code[_index] == '_')
                _index++;
            else
                break;
        }

        var num = _code[start.._index];

        if (num[^1] == '_')
            throw new SyntaxError(start + _offset, _index + _offset, "Invalid number");

        num = num.Replace("_", "");

        try
        {
            var number = @base == 10
                ? double.Parse(num, CultureInfo.InvariantCulture)
                : Convert.ToInt32(num, @base);
            return new Literal(start + _offset, _index + _offset, new NumberLiteral(number));
        }
        catch
        {
            throw new SyntaxError(start + _offset, _index + _offset, "Invalid number");
        }
    }

    private Token GetString()
    {
        var start = _index;

        var symbol = _code[_index];
        var IsRaw = _index <= _code.Length - 3 && _code[_index + 1] == symbol && _code[_index + 2] == symbol;

        var expressions = new List<(int, IExpression)>();

        _index += IsRaw ? 3 : 1;

        var str = new StringBuilder();

        while (true)
        {
            if (_index == _code.Length)
                throw new SyntaxError(start + _offset, _index + _offset, "Missing closing quote");

            if (!IsRaw && _code[_index] == '\n')
                throw new SyntaxError(start + _offset, _index + _offset, "Missing closing quote");

            if (_code[_index] == symbol)
            {
                if (!IsRaw)
                {
                    _index++;
                    break;
                }

                var count = 0;

                while (_index < _code.Length && _code[_index] == symbol)
                {
                    count++;
                    _index++;
                }

                if (count == 1)
                    throw new SyntaxError(_index + _offset, _index + _offset + 1, "unexpected quote");

                if (count % 2 == 0)
                {
                    str.Append(symbol, count / 2);
                }
                else
                {
                    str.Append(symbol, (count - 3) / 2);
                    break;
                }
            }
            else if (!IsRaw && _code[_index] == '\\')
            {
                var character = _code[_index + 1] switch
                {
                    '\"' => '\"',
                    '\'' => '\'',
                    '`' => '`',
                    '\\' => '\\',
                    '0' => '\0',
                    'a' => '\a',
                    'b' => '\b',
                    'f' => '\f',
                    'n' => '\n',
                    'r' => '\r',
                    't' => '\t',
                    'v' => '\v',
                    _ => throw new SyntaxError(_index + _offset, _index + _offset + 2, "unknown escape sequence")
                };

                str.Append(character);
                _index += 2;
            }
            else if (symbol == '`' && _code[_index] == '{')
            {
                if (_code[_index + 1] != '{')
                {
                    var tokens = GetExpression();
                    expressions.Add((str.Length, ExpressionParser.Parse(tokens)));
                }
                else
                {
                    str.Append("{");
                    _index += 2;
                }
            }
            else if (symbol == '`' && _code[_index] == '}')
            {
                if (_code[_index + 1] != '}')
                    throw new SyntaxError(start + _offset, _index + _offset, "unexpected brace");

                str.Append("}");
                _index += 2;
            }
            else
            {
                str.Append(_code[_index]);
                _index++;
            }
        }

        return new Literal(start + _offset, _index + _offset, new StringLiteral(str.ToString(), expressions));

        List<Token> GetExpression()
        {
            var depth = 0;
            var start = _index;

            while (true)
            {
                if (_index == _code.Length)
                    throw new SyntaxError(start + _offset, _index + _offset, "Missing '}'");

                if (_code[_index] == '{')
                    depth++;
                else if (_code[_index] == '}')
                    depth--;

                _index++;

                if (depth == 0)
                    break;
            }

            var text = _code[(start + 1)..(_index - 1)];

            return Scan(text, _offset).ToList();
        }
    }

    private Token GetBlock()
    {
        var symbols = _code[_index] switch
        {
            '(' => new[] { '(', ')' },
            '[' => new[] { '[', ']' },
            '{' => new[] { '{', '}' },
            _ => throw new System.Exception()
        };

        var depth = 0;
        var start = _index;

        while (true)
        {
            if (_index == _code.Length)
                throw new SyntaxError(_index + _offset - 1, _index + _offset, "Missing closing " + symbols[1]);

            if (_code[_index] == symbols[0])
                depth++;
            else if (_code[_index] == symbols[1])
                depth--;

            _index++;

            if (depth == 0)
                break;
        }

        var text = _code.Substring(start + 1, _index - start - 2);

        return new Token(start + _offset, _index + _offset, symbols[0] switch
        {
            '(' => TokenType.Parentheses,
            '[' => TokenType.Brackets,
            '{' => TokenType.Braces,
            _ => throw new Exception()
        }, text);
    }

    private Token GetSymbol()
    {
        var start = _index;

        while (true)
        {
            if (_index == _code.Length)
                break;

            if (Symbol.Symbols.Contains(_code[start..(_index + 1)]))
                _index++;
            else
                break;
        }

        if (start == _index)
            throw new SyntaxError(start + _offset, start + _offset + 1, "unknown operator");

        return new Token(start + _offset, _index + _offset, TokenType.Symbol, _code[start.._index]);
    }

    private Token GetCommand()
    {
        var start = _index++;

        while (true)
        {
            if (_index == _code.Length)
                break;

            if (char.IsWhiteSpace(_code[_index]))
                break;

            if (_code[_index] is '$' or '\'' or '\"' or '`' or ';')
                break;

            if (_index < _code.Length - 1 && _code.Substring(_index, 2) == "|>")
                break;

            _index++;
        }

        var word = _code[start.._index];

        return new Token(start + _offset, _index + _offset, TokenType.Keyword, word);
    }
}