using System.Collections.Generic;

namespace Bloc.Tokens;

internal sealed class StringToken : Token
{
    public string BaseString { get; }
    public List<Interpolation> Interpolations { get; }

    internal StringToken(int start, int end, string baseString, List<Interpolation> interpolations)
        : base(start, end)
    {
        BaseString = baseString;
        Interpolations = interpolations;
    }

    public sealed record Interpolation(int Index, List<IToken> Tokens);
}