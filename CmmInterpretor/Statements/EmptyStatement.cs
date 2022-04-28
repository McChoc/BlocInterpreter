using CmmInterpretor.Data;
using CmmInterpretor.Results;
using CmmInterpretor.Values;

namespace CmmInterpretor.Statements
{
    public class EmptyStatement : Statement
    {
        public override IResult Execute(Call _) => new Void();
    }
}
