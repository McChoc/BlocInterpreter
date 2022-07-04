using CmmInterpretor.Memory;
using CmmInterpretor.Results;

namespace CmmInterpretor.Statements
{
    internal class BreakStatement : Statement
    {
        internal override Result Execute(Call call)
        {
            return new Break();
        }
    }
}