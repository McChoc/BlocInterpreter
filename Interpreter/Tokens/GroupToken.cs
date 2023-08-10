using System.Collections.Generic;

namespace Bloc.Tokens;

internal abstract class GroupToken : Token
{
    public List<Token> Tokens { get; }

    internal GroupToken(int start, int end, List<Token> tokens)
        : base(start, end)
    {
        Tokens = tokens;
    }
}