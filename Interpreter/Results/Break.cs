namespace Bloc.Results;

public sealed class Break : IResult
{
    public string? Label { get; }

    public Break(string? label) => Label = label;
}