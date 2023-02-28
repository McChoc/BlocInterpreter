using System;
using System.Collections.Generic;
using System.Linq;

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

    public override int GetHashCode()
    {
        return HashCode.Combine(BaseString, Interpolations.Count);
    }

    public override bool Equals(object other)
    {
        return other is StringToken token &&
            BaseString == token.BaseString &&
            Interpolations.SequenceEqual(token.Interpolations);
    }

    public sealed class Interpolation
    {
        public int Index { get; }
        public List<Token> Tokens { get; }

        public Interpolation(int index, List<Token> tokens)
        {
            Index = index;
            Tokens = tokens;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Index, Tokens.Count);
        }

        public override bool Equals(object other)
        {
            return other is Interpolation interpolation &&
                Index == interpolation.Index &&
                Tokens.SequenceEqual(interpolation.Tokens);
        }
    }
}