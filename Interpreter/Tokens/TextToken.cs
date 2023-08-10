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
}