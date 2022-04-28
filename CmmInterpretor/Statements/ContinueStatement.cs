using CmmInterpretor.Data;
using CmmInterpretor.Results;

namespace CmmInterpretor.Statements
{
    public class ContinueStatement : Statement
    {
        public override IResult Execute(Call call) => new Continue();
    }
}
