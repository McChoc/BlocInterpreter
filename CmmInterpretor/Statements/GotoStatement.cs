using CmmInterpretor.Memory;
using CmmInterpretor.Results;

namespace CmmInterpretor.Statements
{
    internal class GotoStatement : Statement
    {
        internal new string Label { get; }

        internal GotoStatement(string label) => Label = label;

        internal override Result Execute(Call call) => new Goto(Label);
    }
}
