namespace Bloc.Results;

public sealed class Goto : Result
{
    public string Label { get; }

    public Goto(string label) => Label = label;
}