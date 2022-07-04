namespace CmmInterpretor.Results
{
    public class Goto : Result
    {
        public string label;

        public Goto(string label) => this.label = label;
    }
}