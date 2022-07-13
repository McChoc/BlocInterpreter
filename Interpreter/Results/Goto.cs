namespace Bloc.Results
{
    public class Goto : Result
    {
        public Goto(string label) => Label = label;

        public string Label { get; }
    }
}