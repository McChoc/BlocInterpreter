using Bloc.Utils.Attributes;

namespace Bloc.Tokens;

[Record]
internal sealed partial class LiteralToken : TextToken
{
    internal LiteralToken(int start, int end, string text)
        : base(start, end, text) { }
}