using CmmInterpretor.Data;
using CmmInterpretor.Results;

namespace CmmInterpretor.Statements
{
    public class GotoStatement : Statement
    {
        public string Label { get; set; }

        public GotoStatement(string label) => Label = label;

        public override IResult Execute(Call call) => new Goto(Label);
    }
}
