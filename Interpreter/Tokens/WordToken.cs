using Bloc.Utils.Attributes;

namespace Bloc.Tokens;

[Record]
internal sealed partial class WordToken : TextToken, IKeywordToken, IIdentifierToken
{
    internal WordToken(int start, int end, string text)
        : base(start, end, text) { }
}