using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Bloc.Tokens;
using Bloc.Utils.Constants;
using Bloc.Utils.Exceptions;

namespace Bloc.Scanners;

internal sealed class Tokenizer : ITokenProvider
{
    private static readonly HashSet<char> reservedCharacters = "!\"#$%&'()*+,-./:;<=>?@[\\]^`{|}~".ToHashSet();

    private readonly string _code;
    private readonly int _offset;

    private int _index;

    public Tokenizer(string code, int offset = 0)
    {
        _offset = offset;
        _code = code;
    }

    internal static IEnumerable<Token> Tokenize(string code, int offset = 0)
    {
        var tokenizer = new Tokenizer(code, offset);

        while (tokenizer.HasNext())
            yield return tokenizer.Next();
    }

    public bool HasNext()
    {
        SkipWhiteSpaceAndComments();

        return _index < _code.Length;
    }

    public void Skip(int count = 1)
    {
        for (var i = 0; i < count && HasNext(); i++)
            Next();
    }

    public Token Peek()
    {
        var previousIndex = _index;

        var token = Next();

        _index = previousIndex;

        return token;
    }

    public List<Token> PeekRange(int count)
    {
        var previousIndex = _index;

        var tokens = new List<Token>();

        for (var i = 0; i < count && HasNext(); i++)
            tokens.Add(Next());

        _index = previousIndex;

        return tokens;
    }

    public Token Next()
    {
        if (_index >= _code.Length)
            throw new InvalidOperationException();

        SkipWhiteSpaceAndComments();

        if (IsNumber())
            return GetNumber();

        if (IsString())
            return GetString();

        if (IsParentheses())
            return GetParentheses();

        if (IsBrackets())
            return GetBrackets();

        if (IsBraces())
            return GetBraces();

        if (IsSymbol())
        {
            var symbol = GetSymbol();

            if (symbol is not SymbolToken(Symbol.VARIABLE))
                return symbol;

            if (IsWord())
                return GetWord(true);

            throw new SyntaxError(symbol.Start, symbol.End, "Missing identifier name");
        }

        var word = GetWord(false);

        if (word is KeywordToken(Keyword.NOT) && PeekRange(1) is [KeywordToken(Keyword.IN)])
            return new KeywordToken(word.Start, Next().End, Keyword.NOT_IN);

        if (word is KeywordToken(Keyword.IS) && PeekRange(1) is [KeywordToken(Keyword.NOT)])
            return new KeywordToken(word.Start, Next().End, Keyword.IS_NOT);

        if (word is KeywordToken(Keyword.VAL) && PeekRange(1) is [KeywordToken(Keyword.VAL)])
            return new KeywordToken(word.Start, Next().End, Keyword.VAL_VAL);

        if (word is KeywordToken(Keyword.LET) && PeekRange(1) is [KeywordToken(Keyword.NEW)])
            return new KeywordToken(word.Start, Next().End, Keyword.LET_NEW);

        if (word is KeywordToken(Keyword.CONST) && PeekRange(1) is [KeywordToken(Keyword.NEW)])
            return new KeywordToken(word.Start, Next().End, Keyword.CONST_NEW);

        return word;
    }

    private bool IsNumber()
    {
        if (char.IsDigit(_code[_index]))
            return true;

        if (_index < _code.Length - 1 && _code[_index] == '.' && char.IsDigit(_code[_index + 1]))
            return true;

        return false;
    }

    private bool IsString()
    {
        return _code[_index] is '\'' or '"' or '`';
    }

    private bool IsSymbol()
    {
        return reservedCharacters.Contains(_code[_index]);
    }

    private bool IsParentheses()
    {
        return _code[_index] is '(';
    }

    private bool IsBrackets()
    {
        return _code[_index] is '[';
    }

    private bool IsBraces()
    {
        return _code[_index] is '{';
    }

    private bool IsWord()
    {
        return !(IsNumber() || IsString() || IsSymbol() || IsParentheses() || IsBrackets() || IsBraces());
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

            if (_index < _code.Length - 1 && _code.Substring(_index, 2) == Symbol.COMMENT)
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

    private TextToken GetWord(bool forceIdentifier)
    {
        var start = _index;

        while (_index < _code.Length && !char.IsWhiteSpace(_code[_index]) && !reservedCharacters.Contains(_code[_index]))
            _index++;

        var text = _code[start.._index];

        if (forceIdentifier)
            return new IdentifierToken(start + _offset, _index + _offset, text);
        else if (Keyword.LiteralKeywords.Contains(text))
            return new LiteralToken(start + _offset, _index + _offset, text);
        else if (Keyword.HardKeywords.Contains(text))
            return new KeywordToken(start + _offset, _index + _offset, text);
        else
            return new WordToken(start + _offset, _index + _offset, text);
    }

    private SymbolToken GetSymbol()
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
            throw new SyntaxError(start + _offset, start + _offset + 1, "unknown symbol");

        return new SymbolToken(start + _offset, _index + _offset, _code[start.._index]);
    }

    private NumberToken GetNumber()
    {
        var hasPeriod = false;
        var hasExp = false;

        byte @base = 10;

        if (_code[_index] == '0' && _index < _code.Length - 1)
        {
            @base = char.ToLower(_code[_index + 1]) switch
            {
                'b' => 2,
                'o' => 8,
                'x' => 16,
                 _  => 10,
            };

            if (@base != 10)
                _index += 2;
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
            {
                _index++;
                continue;
            }

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

            return new NumberToken(start + _offset, _index + _offset, number);
        }
        catch
        {
            throw new SyntaxError(start + _offset, _index + _offset, "Invalid number");
        }
    }

    private StringToken GetString()
    {
        var start = _index;

        var symbol = _code[_index];
        var IsRaw = _index <= _code.Length - 3 && _code[_index + 1] == symbol && _code[_index + 2] == symbol;

        var interpolations = new List<StringToken.Interpolation>();

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
                    '`' => '`',
                    's' => ' ',
                    '0' => '\0',
                    'a' => '\a',
                    'b' => '\b',
                    'f' => '\f',
                    'n' => '\n',
                    'r' => '\r',
                    't' => '\t',
                    'v' => '\v',
                    '\"' => '\"',
                    '\'' => '\'',
                    '\\' => '\\',
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
                    interpolations.Add(new(str.Length, tokens));
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

        return new StringToken(start + _offset, _index + _offset, str.ToString(), interpolations);

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

            return Tokenize(text, _offset).ToList();
        }
    }

    private Token GetParentheses()
    {
        var start = _index;
        var text = GetBlock();
        var tokens = Tokenize(text, start + _offset).ToList();

        return new GroupToken(start, _index, tokens);
    }

    private Token GetBrackets()
    {
        var start = _index;
        var text = GetBlock();
        var tokens = Tokenize(text, start + _offset).ToList();

        return new IndexerToken(start, _index, tokens);
    }

    private Token GetBraces()
    {
        var start = _index;
        var text = GetBlock();
        var tokens = Tokenize(text, start + _offset).ToList();

        if (IsCodeBlock(tokens))
            return new CodeBlockToken(start, _index, tokens);

        if (IsStruct(tokens))
            return new StructToken(start, _index, tokens);

        return new ArrayToken(start, _index, tokens);
    }

    private string GetBlock()
    {
        var (open, close) = _code[_index] switch
        {
            '(' => ('(', ')'),
            '[' => ('[', ']'),
            '{' => ('{', '}'),
            _ => throw new Exception()
        };

        var depth = 0;
        var start = _index;

        while (true)
        {
            if (_index == _code.Length)
                throw new SyntaxError(_index + _offset - 1, _index + _offset, $"Missing closing {close}");

            if (_code[_index] == open)
                depth++;
            else if (_code[_index] == close)
                depth--;

            _index++;

            if (depth == 0)
                break;
        }

        return _code[(start + 1)..(_index - 1)];
    }

    private static bool IsStruct(List<Token> tokens)
    {
        var token = tokens.FirstOrDefault(x => x is SymbolToken(Symbol.COMMA or Symbol.ASSIGN or Symbol.UNPACK_ARRAY or Symbol.UNPACK_STRUCT));

        return token is SymbolToken(Symbol.ASSIGN or Symbol.UNPACK_STRUCT);
    }

    private static bool IsCodeBlock(List<Token> tokens)
    {
        return tokens switch
        {
            [] => true,
            [CodeBlockToken, ..] => true,
            [KeywordToken token, ..] when IsControlFlowKeyword(token) || IsLoopKeyword (token) => true,
            [WordToken(Keyword.UNCHECKED), KeywordToken token, ..] when IsLoopKeyword(token) => true,
            [IIdentifierToken, SymbolToken(Symbol.COLON), CodeBlockToken, ..] => true,
            [IIdentifierToken, SymbolToken(Symbol.COLON), KeywordToken token, ..] when IsControlFlowKeyword(token) || IsLoopKeyword(token) => true,
            [IIdentifierToken, SymbolToken(Symbol.COLON), WordToken(Keyword.UNCHECKED), KeywordToken token, ..] when IsLoopKeyword(token) => true,
            _ when tokens.Any(x => x is SymbolToken(Symbol.SEMICOLON)) => true,
            _ => false,
        };

        static bool IsControlFlowKeyword(KeywordToken token)
        {
            return token.Text is
                Keyword.IF or
                Keyword.UNLESS or
                Keyword.LOCK or
                Keyword.TRY;
        }

        static bool IsLoopKeyword(KeywordToken token)
        {
            return token.Text is
                Keyword.DO or
                Keyword.WHILE or
                Keyword.UNTIL or
                Keyword.LOOP or
                Keyword.REPEAT or
                Keyword.FOR or
                Keyword.FOREACH;
        }
    }
}