using Bloc.Utils.Attributes;

namespace Bloc.Tokens;

[Record]
internal sealed partial class KeywordToken : TextToken, IKeywordToken
{
    internal KeywordToken(int start, int end, string text)
        : base(start, end, text) { }
}