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
    private readonly string _code;
    private readonly int _offset;

    private int _index;

    public Tokenizer(string code, int offset = 0)
    {
        _offset = offset;
        _code = code;
    }

    internal static IEnumerable<IToken> Tokenize(string code, int offset = 0)
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
        for (int i = 0; i < count && HasNext(); i++)
            Next();
    }

    public IToken Peek()
    {
        int previousIndex = _index;

        var token = Next();

        _index = previousIndex;

        return token;
    }

    public List<IToken> PeekRange(int count)
    {
        int previousIndex = _index;

        var tokens = new List<IToken>();

        for (int i = 0; i < count && HasNext(); i++)
            tokens.Add(Next());

        _index = previousIndex;

        return tokens;
    }

    public IToken Next()
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

            if (symbol is not SymbolToken(Symbol.DOLAR))
                return symbol;

            if (IsParentheses())
                return GetDynamicIdentifier();

            if (IsWord())
                return GetWord(true);

            throw new SyntaxError(symbol.Start, symbol.End, "Unexpected token");
        }

        var word = GetWord(false);

        if (PeekRange(1) is not [var next])
            return word;

        return (word, next) switch
        {
            (KeywordToken(Keyword.IS), KeywordToken(Keyword.NOT)) => new KeywordToken(word.Start, Next().End, Keyword.IS_NOT),
            (KeywordToken(Keyword.NOT), KeywordToken(Keyword.IN)) => new KeywordToken(word.Start, Next().End, Keyword.NOT_IN),
            (KeywordToken(Keyword.LET), KeywordToken(Keyword.NEW)) => new KeywordToken(word.Start, Next().End, Keyword.LET_NEW),
            (KeywordToken(Keyword.CONST), KeywordToken(Keyword.NEW)) => new KeywordToken(word.Start, Next().End, Keyword.CONST_NEW),
            (KeywordToken(Keyword.SELECT), SymbolToken(Symbol.STAR)) => new KeywordToken(word.Start, Next().End, Keyword.SELECT_MANY),
            _ => word
        };
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
        return Character.ReservedCharacters.Contains(_code[_index]);
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
        int start = _index;
        bool skip;

        do
        {
            skip = false;

            if (_index < _code.Length - 1 && _code.Substring(_index, 2) == Symbol.L_COMMENT)
            {
                skip = true;

                _index += 2;

                while (true)
                {
                    if (_index >= _code.Length - 1)
                        throw new SyntaxError(start, start + 2, $"missing '{Symbol.R_COMMENT}'");

                    if (_code.Substring(_index, 2) == Symbol.R_COMMENT)
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
        int start = _index;

        while (_index < _code.Length && !char.IsWhiteSpace(_code[_index]) && !Character.ReservedCharacters.Contains(_code[_index]))
            _index++;

        string text = _code[start.._index];

        if (forceIdentifier)
            return new StaticIdentifierToken(start + _offset, _index + _offset, text);
        else if (Keyword.LiteralKeywords.Contains(text))
            return new LiteralKeywordToken(start + _offset, _index + _offset, text);
        else if (Keyword.HardKeywords.Contains(text))
            return new KeywordToken(start + _offset, _index + _offset, text);
        else
            return new WordToken(start + _offset, _index + _offset, text);
    }

    private SymbolToken GetSymbol()
    {
        int start = _index;
        int end = _index;

        while (_index < _code.Length && Character.ReservedCharacters.Contains(_code[_index]))
            if (Symbol.Symbols.Contains(_code[start..++_index]))
                end = _index;

        if (start == end)
            throw new SyntaxError(start + _offset, start + _offset + 1, "unknown symbol");

        _index = end;

        return new SymbolToken(start + _offset, end + _offset, _code[start..end]);
    }

    private NumberToken GetNumber()
    {
        int expIndex = -1;
        int dotIndex = -1;

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

        int start = _index;

        do
        {
            switch (char.ToLower(_code[_index]))
            {
                case '.' when dotIndex == -1 && @base == 10:
                    dotIndex = _index;
                    continue;

                case 'e' when expIndex == -1 && @base == 10:
                    expIndex = _index;
                    continue;

                case '_':
                case '0' or '1' when @base >= 2:
                case '2' or '3' or '4' or '5' or '6' or '7' when @base >= 8:
                case '8' or '9' when @base >= 10:
                case 'a' or 'b' or 'c' or 'd' or 'e' or 'f' when @base >= 16:
                case '+' or '-' when expIndex == _index - 1:
                    continue;
            }

            break;
        }
        while (++_index < _code.Length);

        if (dotIndex == _index - 1)
            _index--;

        string text = _code[start.._index].Replace("_", "");

        if (text == "")
            throw new SyntaxError(start + _offset, _index + _offset, "Invalid number");

        double number = @base == 10
            ? double.Parse(text, CultureInfo.InvariantCulture)
            : Convert.ToInt32(text, @base);

        return new NumberToken(start + _offset, _index + _offset, number);
    }

    private StringToken GetString()
    {
        int start = _index;

        char symbol = _code[_index];
        bool IsRaw = _index <= _code.Length - 3 && _code[_index + 1] == symbol && _code[_index + 2] == symbol;

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

                int count = 0;

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
                char character = _code[_index + 1] switch
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

        List<IToken> GetExpression()
        {
            int depth = 0;
            int start = _index;

            do
            {
                if (_index == _code.Length)
                    throw new SyntaxError(start + _offset, _index + _offset, "Missing '}'");

                if (_code[_index] == '{')
                    depth++;
                else if (_code[_index] == '}')
                    depth--;

                _index++;
            }
            while (depth > 0);

            string text = _code[(start + 1)..(_index - 1)];

            return Tokenize(text, _offset).ToList();
        }
    }

    private IToken GetDynamicIdentifier()
    {
        int start = _index;
        string text = GetBlock();
        var tokens = Tokenize(text, start + _offset).ToList();

        return new DynamicIdentifierToken(start, _index, tokens);
    }

    private IToken GetParentheses()
    {
        int start = _index;
        string text = GetBlock();
        var tokens = Tokenize(text, start + _offset).ToList();

        return new ParenthesesToken(start, _index, tokens);
    }

    private IToken GetBrackets()
    {
        int start = _index;
        string text = GetBlock();
        var tokens = Tokenize(text, start + _offset).ToList();

        return new BracketsToken(start, _index, tokens);
    }

    private IToken GetBraces()
    {
        int start = _index;
        string text = GetBlock();
        var tokens = Tokenize(text, start + _offset).ToList();

        return new BracesToken(start, _index, tokens);
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

        int depth = 0;
        int start = _index;

        do
        {
            if (_index == _code.Length)
                throw new SyntaxError(_index + _offset - 1, _index + _offset, $"Missing closing {close}");

            if (_code[_index] == open)
                depth++;
            else if (_code[_index] == close)
                depth--;

            _index++;
        }
        while (depth > 0);

        return _code[(start + 1)..(_index - 1)];
    }
}