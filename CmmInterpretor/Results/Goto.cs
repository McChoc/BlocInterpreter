namespace CmmInterpretor.Results
{
    public class Goto : IResult
    {
        public string label;

        public Goto(string label) => this.label = label;
    }
}
