using CmmInterpretor.Memory;
using CmmInterpretor.Results;

namespace CmmInterpretor.Statements
{
    public class ContinueStatement : Statement
    {
        public override Result Execute(Call call) => new Continue();
    }
}
