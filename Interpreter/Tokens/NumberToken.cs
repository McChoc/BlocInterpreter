using Bloc.Utils.Attributes;

namespace Bloc.Tokens;

[Record]
internal sealed partial class NumberToken : Token
{
    public double Number { get; }

    internal NumberToken(int start, int end, double number)
        : base (start, end)
    {
        Number = number;
    }
}