using System.Collections.Generic;

namespace Bloc.Tokens;

internal sealed class BracketsToken : GroupToken
{
    internal BracketsToken(int start, int end, List<Token> tokens)
        : base(start, end, tokens) { }
}