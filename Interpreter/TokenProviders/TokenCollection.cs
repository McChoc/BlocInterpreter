using System.Collections.Generic;
using Bloc.Tokens;
using Bloc.Utils.Extensions;

namespace Bloc.Scanners;

internal sealed class TokenCollection : ITokenProvider
{
    private int _index;
    private readonly List<IToken> _tokens;

    public TokenCollection(List<IToken> tokens)
    {
        _tokens = tokens;
    }

    public void Skip(int count = 1) => _index += count;

    public bool HasNext() => _index < _tokens.Count;

    public IToken Next() => _tokens[_index++];

    public IToken Peek() => _tokens[_index];

    public List<IToken> PeekRange(int count)
    {
        return _index + count > _tokens.Count
            ? _tokens.GetRange(_index..)
            : _tokens.GetRange(_index, count);
    }
}