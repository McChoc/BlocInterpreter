using CmmInterpretor.Memory;
using CmmInterpretor.Results;

namespace CmmInterpretor.Statements
{
    public class BreakStatement : Statement
    {
        public override Result Execute(Call call) => new Break();
    }
}
