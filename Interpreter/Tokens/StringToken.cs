using System.Collections.Generic;
using Bloc.Utils.Attributes;

namespace Bloc.Tokens;

[Record]
internal sealed partial class StringToken : Token
{
    public string BaseString { get; }
    public List<Interpolation> Interpolations { get; }

    internal StringToken(int start, int end, string baseString, List<Interpolation> interpolations)
        : base(start, end)
    {
        BaseString = baseString;
        Interpolations = interpolations;
    }

    [Record]
    public sealed partial class Interpolation
    {
        public int Index { get; }
        public List<Token> Tokens { get; }

        public Interpolation(int index, List<Token> tokens)
        {
            Index = index;
            Tokens = tokens;
        }
    }
}