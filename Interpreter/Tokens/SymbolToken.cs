using Bloc.Utils.Attributes;

namespace Bloc.Tokens;

[Record]
internal sealed partial class SymbolToken : TextToken
{
    internal SymbolToken(int start, int end, string text)
        : base(start, end, text) { }
}