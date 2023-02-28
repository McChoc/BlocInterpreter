namespace Bloc.Tokens;

internal sealed class LiteralToken : TextToken
{
    internal LiteralToken(int start, int end, string text)
        : base(start, end, text) { }
}