namespace Bloc.Tokens;

internal sealed class LiteralKeywordToken : TextToken
{
    internal LiteralKeywordToken(int start, int end, string text)
        : base(start, end, text) { }
}