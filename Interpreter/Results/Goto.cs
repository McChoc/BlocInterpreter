namespace Bloc.Results
{
    public sealed class Goto : Result
    {
        public Goto(string label) => Label = label;

        public string Label { get; }
    }
}