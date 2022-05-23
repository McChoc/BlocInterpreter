using CmmInterpretor.Memory;
using CmmInterpretor.Results;

namespace CmmInterpretor.Statements
{
    public class GotoStatement : Statement
    {
        public new string Label { get; }

        public GotoStatement(string label) => Label = label;

        public override Result Execute(Call call) => new Goto(Label);
    }
}
