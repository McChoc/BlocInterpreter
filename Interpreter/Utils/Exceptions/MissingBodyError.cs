namespace Bloc.Utils.Exceptions;

public sealed class MissingBodyError : SyntaxError
{
    public override bool Fatal { get; }

    public MissingBodyError(int start, int end, bool fatal = true)
        : base(start, end, "Missing body")
        => Fatal = fatal;
}