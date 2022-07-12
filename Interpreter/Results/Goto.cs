namespace Bloc.Results
{
    public class Goto : Result
    {
        public string Label { get; }

        public Goto(string label) => Label = label;
    }
}