using System;

namespace Bloc.Utils.Exceptions;

public class SyntaxError : Exception
{
    public int Start { get; }
    public int End { get; }
    public string Text { get; }
    public virtual bool Fatal => true;

    public SyntaxError(int start, int end, string text) : base(text)
    {
        Start = start;
        End = end;
        Text = text;
    }
}