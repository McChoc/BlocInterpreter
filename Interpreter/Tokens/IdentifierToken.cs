using Bloc.Utils.Attributes;

namespace Bloc.Tokens;

[Record]
internal sealed partial class IdentifierToken : TextToken, IIdentifierToken
{
    internal IdentifierToken(int start, int end, string text)
        : base(start, end, text) { }
}