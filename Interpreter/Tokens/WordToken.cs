namespace Bloc.Tokens;

internal sealed class WordToken : TextToken, IKeywordToken, IIdentifierToken
{
    internal WordToken(int start, int end, string text)
        : base(start, end, text) { }
}