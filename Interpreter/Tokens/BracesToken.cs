using System.Collections.Generic;

namespace Bloc.Tokens;

internal sealed class BracesToken : GroupToken
{
    internal BracesToken(int start, int end, List<Token> tokens)
        : base(start, end, tokens) { }
}