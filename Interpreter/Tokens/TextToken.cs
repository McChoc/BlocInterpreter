using System;

namespace Bloc.Tokens;

internal abstract class TextToken : Token
{
    public string Text { get; }

    internal TextToken(int start, int end, string text)
        : base(start, end)
    {
        Text = text;
    }

    public void Deconstruct(out string text)
    {
        text = Text;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Text);
    }

    public override bool Equals(object other)
    {
        return other is TextToken token &&
            GetType() == token.GetType() &&
            Text == token.Text;
    }
}