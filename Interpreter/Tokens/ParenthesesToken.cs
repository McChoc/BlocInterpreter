using System.Collections.Generic;
using Bloc.Utils.Attributes;

namespace Bloc.Tokens;

[Record]
internal sealed partial class ParenthesesToken : GroupToken
{
    internal ParenthesesToken(int start, int end, List<Token> tokens)
        : base(start, end, tokens) { }
}