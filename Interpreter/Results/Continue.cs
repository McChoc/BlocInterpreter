namespace Bloc.Results;

public sealed class Continue : IResult
{
    public string? Label { get; }

    public Continue(string? label) => Label = label;
}