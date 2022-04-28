using CmmInterpretor.Data;
using CmmInterpretor.Results;

namespace CmmInterpretor.Statements
{
    public class BreakStatement : Statement
    {
        public override IResult Execute(Call call) => new Break();
    }
}
