using CmmInterpretor.Data;
using CmmInterpretor.Values;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using CmmInterpretor.Exceptions;

namespace CmmInterpretor.Tokens
{
    public class TokenScanner
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
            "-", "--", "-=", ",", ";", ":", "::", ":=",
            "!", "!!", "!=", "?", ".", "..", "@", "@@",
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
            "in", "is", "as",
            "val", "ref", "new",
            "async", "await",
            "nameof", "typeof",
            "def", "delete",
            "import", "if", "else",
            "do", "while", "until",
            "loop", "repeat", "for",
            "try", "catch", "finally",
            "return", "exit", "throw",
            "continue", "break", "goto",
            "lock", "with", "pass"
        };

        private int _index;
        private readonly string _code;

        public TokenScanner(string code) => _code = code;

        public bool HasNextToken()
        {
            SkipWhiteSpace();

            return _index < _code.Length;
        }

        public Token Peek()
        {
            int previousIndex = _index;

            var token = GetNextToken();

            _index = previousIndex;

            return token;
        }

        public List<Token> Peek(int count)
        {
            int previousIndex = _index;

            var tokens = new List<Token>();

            for (int i = 0; i < count && HasNextToken(); i++)
                tokens.Add(GetNextToken());

            _index = previousIndex;

            return tokens;
        }

        public Token GetNextToken()
        {
            if (_index >= _code.Length)
                throw new System.Exception();

            SkipWhiteSpace();

            if (char.IsDigit(_code[_index]) || (_code[_index] == '.' && _index < _code.Length - 1 && char.IsDigit(_code[_index + 1])))
                return GetNumber();
            else if (_code[_index] is '\'' or '"' or '`')
                return GetString();
            else if (_code[_index] is '(' or '{' or '[')
                return GetBlock();
            if (symbols.Contains(_code[_index]) && _code[_index] != '$')
                return GetOperator();
            else
                return GetWord();
        }

        public Token GetNextCommandToken()
        {
            if (_index >= _code.Length)
                throw new System.Exception();

            SkipWhiteSpace();

            if (_index < _code.Length - 1 && _code.Substring(_index, 2) == "|>")
            {
                _index += 2;
                return new Token(TokenType.Operator, "|>");
            }
            else if (_code[_index] == ';')
            {
                _index++;
                return new Token(TokenType.Operator, ";");
            }
            else if (_code[_index] == '$')
            {
                return GetWord();
            }
            else if (_code[_index] is '\'' or '"' or '`')
            {
                return GetString();
            }
            else
            {
                return GetCommand();
            }
        }

        private void SkipWhiteSpace()
        {
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
                            throw new SyntaxError("missing '*#'");

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
            }
            while (skip);
        }

        private Token GetWord()
        {
            int start = _index++;

            while (true)
            {
                if (_index == _code.Length)
                    break;

                if (char.IsWhiteSpace(_code[_index]) || symbols.Contains(_code[_index]))
                    break;

                _index++;
            }

            string word = _code[start.._index];

            if (word == "$")
                throw new SyntaxError("Missing variable name.");

            if (keyWords.Contains(word))
                return new Token(TokenType.Keyword, word);

            return word switch
            {
                "void" => new Token(TokenType.Literal, Void.Value),
                "null" => new Token(TokenType.Literal, Null.Value),

                "false" => new Token(TokenType.Literal, Bool.False),
                "true" => new Token(TokenType.Literal, Bool.True),
                "nan" => new Token(TokenType.Literal, Number.NaN),
                "infinity" => new Token(TokenType.Literal, Number.Infinity),

                "bool" => new Token(TokenType.Literal, new TypeCollection(VariableType.Bool)),
                "number" => new Token(TokenType.Literal, new TypeCollection(VariableType.Number)),
                "range" => new Token(TokenType.Literal, new TypeCollection(VariableType.Range)),
                "string" => new Token(TokenType.Literal, new TypeCollection(VariableType.String)),
                "tuple" => new Token(TokenType.Literal, new TypeCollection(VariableType.Tuple)),
                "array" => new Token(TokenType.Literal, new TypeCollection(VariableType.Array)),
                "struct" => new Token(TokenType.Literal, new TypeCollection(VariableType.Struct)),
                "function" => new Token(TokenType.Literal, new TypeCollection(VariableType.Function)),
                "task" => new Token(TokenType.Literal, new TypeCollection(VariableType.Task)),
                "reference" => new Token(TokenType.Literal, new TypeCollection(VariableType.Reference)),
                "complex" => new Token(TokenType.Literal, new TypeCollection(VariableType.Complex)),
                "type" => new Token(TokenType.Literal, new TypeCollection(VariableType.Type)),
                "any" => new Token(TokenType.Literal, TypeCollection.Any),

                _ => new Token(TokenType.Identifier, word.TrimStart('$')),
            };
        }

        private Token GetNumber()
        {
            bool hasPeriod = false;
            bool hasExp = false;

            byte @base = 10;

            if (_code[_index] == '0' && _index < _code.Length - 1)
            {
                switch (_code[_index + 1])
                {
                    case 'b': @base = 2; _index += 2; break;
                    case 'o': @base = 8; _index += 2; break;
                    case 'x': @base = 16; _index += 2; break;
                }
            }

            int start = _index;

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

                if ((_code[_index] == '+' || _code[_index] == '-') && _index > 0 && char.ToUpper(_code[_index - 1]) == 'E' && @base == 10)
                {
                    _index++;
                    continue;
                }

                if (char.IsLetterOrDigit(_code[_index]) || _code[_index] == '_')
                    _index++;
                else
                    break;
            }

            string num = _code[start.._index];

            if (num[^1] == '.')
            {
                num = num[..^1];
                _index--;
            }

            if (num[^1] == '_')
                throw new SyntaxError("Invalid number");

            num = num.Replace("_", "");

            try
            {
                if (@base == 10)
                    return new Token(TokenType.Literal, new Number(double.Parse(num, CultureInfo.InvariantCulture)));
                else
                    return new Token(TokenType.Literal, new Number(System.Convert.ToInt32(num, @base)));
            }
            catch
            {
                throw new SyntaxError("Invalid number");
            }
        }

        private Token GetString()
        {
            char symbol = _code[_index];
            bool IsRaw = _index <= _code.Length - 3 && _code[_index + 1] == symbol && _code[_index + 2] == symbol;

            var tokens = new List<Token>();

            _index += IsRaw ? 3 : 1;

            var str = new StringBuilder();

            while (true)
            {
                if (_index == _code.Length)
                    throw new SyntaxError("");

                if (!IsRaw && _code[_index] == '\n')
                    throw new SyntaxError("");

                if (!IsRaw && _code[_index] == symbol)
                {
                    _index++;
                    break;
                }
                else if (IsRaw && _index <= _code.Length - 3 && _code[_index] == symbol && _code[_index + 1] == symbol && _code[_index + 2] == symbol)
                {
                    _index += 3;
                    break;
                }
                else if (!IsRaw && _code[_index] == '\\')
                {
                    var c = _code[_index + 1] switch
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
                        _ => throw new SyntaxError(""),
                    };

                    str.Append(c);

                    _index += 2;
                }
                else if (symbol == '`' && _code[_index] == '{')
                {
                    if (_code[_index + 1] == '{')
                    {
                        str.Append("{{");
                        _index += 2;
                    }
                    else
                    {
                        str.Append($"{{{tokens.Count}}}");
                        tokens.Add(GetBlock(true));
                    }
                }
                else if (symbol == '`' && _code[_index] == '}')
                {
                    if (_code[_index + 1] == '}')
                    {
                        str.Append("}}");
                        _index += 2;
                    }
                    else
                    {
                        throw new SyntaxError("");
                    }
                }
                else
                {
                    str.Append(_code[_index]);
                    _index++;
                }
            }

            if (symbol == '`')
                return new Token(TokenType.Interpolated, (str.ToString(), tokens));

            return new Token(TokenType.Literal, new String(str.ToString()));
        }

        private Token GetBlock(bool expression = false)
        {
            char[] symbols = _code[_index] switch
            {
                '(' => new[] { '(', ')' },
                '[' => new[] { '[', ']' },
                '{' => new[] { '{', '}' },
                _ => throw new System.Exception()
            };

            int depth = 0;
            int start = _index;

            while (true)
            {
                if (_index == _code.Length)
                    throw new SyntaxError("");

                if (_code[_index] == symbols[0])
                    depth++;
                else if (_code[_index] == symbols[1])
                    depth--;

                _index++;

                if (depth == 0)
                    break;
            }

            string text = _code.Substring(start + 1, _index - start - 2);

            var scanner = new TokenScanner(text);
            var tokens = new List<Token>();

            while (scanner.HasNextToken())
                tokens.Add(scanner.GetNextToken());

            return symbols[0] switch
            {
                '(' => new Token(TokenType.Parentheses, tokens),
                '[' => new Token(TokenType.Brackets, tokens),
                '{' => new Token(TokenType.Block, expression ? tokens : text),
                _ => throw new System.Exception()
            };
        }

        private Token GetOperator()
        {
            int start = _index;

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
                throw new SyntaxError("unknown symbol");

            return new Token(TokenType.Operator, _code[start.._index]);
        }

        private Token GetCommand()
        {
            int start = _index++;

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

            string word = _code[start.._index];

            return new Token(TokenType.Command, word);
        }
    }
}
