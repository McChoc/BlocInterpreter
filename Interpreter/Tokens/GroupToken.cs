using System.Collections.Generic;

namespace Bloc.Tokens;

internal abstract class GroupToken : Token
{
    public List<IToken> Tokens { get; }

    internal GroupToken(int start, int end, List<IToken> tokens)
        : base(start, end)
    {
        Tokens = tokens;
    }
}