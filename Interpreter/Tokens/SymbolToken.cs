namespace Bloc.Tokens;

internal sealed class SymbolToken : TextToken
{
    internal SymbolToken(int start, int end, string text)
        : base(start, end, text) { }
}