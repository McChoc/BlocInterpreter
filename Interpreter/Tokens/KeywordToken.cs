namespace Bloc.Tokens;

internal sealed class KeywordToken : TextToken, IKeywordToken
{
    internal KeywordToken(int start, int end, string text)
        : base(start, end, text) { }
}