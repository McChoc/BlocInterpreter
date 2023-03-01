using System.Collections.Generic;
using Bloc.Tokens;
using Bloc.Utils.Extensions;

namespace Bloc.Scanners;

internal class TokenCollection : ITokenProvider
{
    private readonly List<Token> _tokens;

    private int _index;

    public TokenCollection(List<Token> tokens) => _tokens = tokens;

    public void Skip(int count = 1) => _index += count;

    public bool HasNext() => _index < _tokens.Count;

    public Token Next() => _tokens[_index++];

    public Token Peek() => _tokens[_index];

    public List<Token> PeekRange(int count)
    {
        return _index + count > _tokens.Count
            ? _tokens.GetRange(_index..)
            : _tokens.GetRange(_index, count);
    }
}