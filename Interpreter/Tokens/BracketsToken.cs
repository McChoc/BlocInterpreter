using System.Collections.Generic;

namespace Bloc.Tokens;

internal sealed class BracketsToken : GroupToken
{
    internal BracketsToken(int start, int end, List<IToken> tokens)
        : base(start, end, tokens) { }

    internal enum ContentType
    {
        ArrayPattern,
        StructPattern
    }
}