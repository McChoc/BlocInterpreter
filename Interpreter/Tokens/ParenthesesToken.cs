using System.Collections.Generic;

namespace Bloc.Tokens;

internal sealed class ParenthesesToken : GroupToken
{
    internal ParenthesesToken(int start, int end, List<Token> tokens)
        : base(start, end, tokens) { }
}