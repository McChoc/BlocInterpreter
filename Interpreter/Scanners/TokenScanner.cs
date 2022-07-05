using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Bloc.Expressions;
using Bloc.Tokens;
using Bloc.Utils.Exceptions;

namespace Bloc.Scanners
{
    internal class TokenScanner
    {
        private static readonly HashSet<char> symbols = new()
        {
            '=', '<', '>', '!', '~', '&', '|', '^',
            '+', '-', '*', '/', '%', '?', ':', '.', ',', ';',
            '@', '$', '#', '\\', '\'', '\"', '`',
            '(', ')', '[', ']', '{', '}'
        };

        private static readonly HashSet<string> operators = new()
        {
            "-", "--", "-=", ",", ";", ":", "::",
            "!", "!!", "!=", "?", ".", "..",
            "*", "**", "**=", "*=", "/", "//", "//=", "/=",
            "&", "&&", "&&=", "&=", "%", "%%", "%%=", "%=",
            "^", "^^", "^^=", "^=", "+", "++", "+=",
            "<", "<<", "<<=", "<=", "<=>", "<>", "=", "==", "=>",
            ">", ">=", ">>", ">>=", "|", "|=", "|>", "||", "||=",
            "~", "~~"
        };

        private static readonly HashSet<string> keyWords = new()
        {
            "chr", "ord", "len",
            "not", "in", "is", "as",
            "val", "ref", "new",
            "nameof", "typeof",
            "await", "async",
            "let", "delete",
            "var", "if", "else", "lock",
            "do", "while", "until",
            "loop", "repeat", "for",
            "try", "catch", "finally",
            "throw", "return", "exit",
            "continue", "break", "goto",
            "pass", "recall", "params",
            "void", "null", "false",
            "true", "infinity", "nan",
            "bool", "number", "range",
            "string", "array", "struct",
            "tuple", "function", "task",
            "reference", "complex", "type"
        };

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
            SkipWhiteSpace();

            return _index < _code.Length;
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
                throw new Exception();

            SkipWhiteSpace();

            if (char.IsDigit(_code[_index]) ||
                (_code[_index] == '.' && _index < _code.Length - 1 && char.IsDigit(_code[_index + 1])))
                return GetNumber();

            if (_code[_index] is '\'' or '"' or '`')
                return GetString();

            if (_code[_index] is '(' or '{' or '[')
                return GetBlock();

            if (symbols.Contains(_code[_index]) && _code[_index] != '$')
                return GetOperator();

            var word = GetWord();

            if (word is (TokenType.Keyword, "not") && HasNextToken() && Peek() is (TokenType.Keyword, "in"))
                return new Token(word.Start, GetNextToken().End, TokenType.Keyword, "not in");

            if (word is (TokenType.Keyword, "is") && HasNextToken() && Peek() is (TokenType.Keyword, "not"))
                return new Token(word.Start, GetNextToken().End, TokenType.Keyword, "is not");

            return word;
        }

        internal Token GetNextCommandToken()
        {
            if (_index >= _code.Length)
                throw new Exception();

            SkipWhiteSpace();

            if (_index < _code.Length - 1 && _code.Substring(_index, 2) == "|>")
                return new Token(_index + _offset, (_index += 2) + _offset, TokenType.Operator, "|>");

            if (_code[_index] == ';')
                return new Token(_index + _offset, ++_index + _offset, TokenType.Operator, ";");

            if (_code[_index] == '$')
                return GetWord();

            if (_code[_index] is '\'' or '"' or '`')
                return GetString();

            return GetCommand();
        }

        private void SkipWhiteSpace()
        {
            var start = _index;
            bool skip;

            do
            {
                skip = false;

                if (_index < _code.Length - 1 && _code.Substring(_index, 2) == "#*")
                {
                    skip = true;

                    _index += 2;

                    while (true)
                    {
                        if (_index >= _code.Length - 1)
                            throw new SyntaxError(start, start + 2, "missing '*#'");

                        if (_code.Substring(_index, 2) == "*#")
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

        private Token GetWord()
        {
            var start = _index++;

            while (true)
            {
                if (_index == _code.Length)
                    break;

                if (char.IsWhiteSpace(_code[_index]) || symbols.Contains(_code[_index]))
                    break;

                _index++;
            }

            var word = _code[start.._index];

            if (word == "$")
                throw new SyntaxError(start + _offset, _index + _offset, "Missing variable name.");

            if (keyWords.Contains(word))
                return new Token(start + _offset, _index + _offset, TokenType.Keyword, word);

            return new Token(start + _offset, _index + _offset, TokenType.Identifier, word.TrimStart('$'));
        }

        private Token GetNumber()
        {
            var hasPeriod = false;
            var hasExp = false;

            byte @base = 10;

            if (_code[_index] == '0' && _index < _code.Length - 1)
                switch (_code[_index + 1])
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

                if (char.ToUpper(_code[_index]) == 'E' && !hasExp && @base == 10)
                {
                    _index++;
                    hasExp = true;
                    continue;
                }

                if ((_code[_index] == '+' || _code[_index] == '-') && _index > 0 &&
                    char.ToUpper(_code[_index - 1]) == 'E' && @base == 10)
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

            if (num[^1] == '.')
            {
                num = num[..^1];
                _index--;
            }

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
                _ => throw new Exception()
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

        private Token GetOperator()
        {
            var start = _index;

            while (true)
            {
                if (_index == _code.Length)
                    break;

                if (operators.Contains(_code[start..(_index + 1)]))
                    _index++;
                else
                    break;
            }

            if (start == _index)
                throw new SyntaxError(start + _offset, start + _offset + 1, "unknown operator");

            return new Token(start + _offset, _index + _offset, TokenType.Operator, _code[start.._index]);
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
}