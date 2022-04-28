using CmmInterpretor.Data;
using CmmInterpretor.Results;
using CmmInterpretor.Values;

namespace CmmInterpretor.Statements
{
    public class LabelStatement : Statement
    {
        public string Label { get; set; }

        public LabelStatement(string label) => Label = label;

        public override IResult Execute(Call _) => new Void();
    }
}
