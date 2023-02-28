namespace Bloc.Tokens;

internal sealed class IdentifierToken : TextToken, IIdentifierToken
{
    internal IdentifierToken(int start, int end, string text)
        : base(start, end, text) { }
}