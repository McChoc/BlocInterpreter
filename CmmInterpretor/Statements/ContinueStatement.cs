using CmmInterpretor.Memory;
using CmmInterpretor.Results;

namespace CmmInterpretor.Statements
{
    internal class ContinueStatement : Statement
    {
        internal override Result Execute(Call call)
        {
            return new Continue();
        }
    }
}