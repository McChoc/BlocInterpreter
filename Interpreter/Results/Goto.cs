namespace Bloc.Results;

public sealed class Goto : IResult
{
    public string Label { get; }

    public Goto(string label) => Label = label;
}