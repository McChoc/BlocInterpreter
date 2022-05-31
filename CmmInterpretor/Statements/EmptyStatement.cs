using CmmInterpretor.Memory;
using CmmInterpretor.Results;

namespace CmmInterpretor.Statements
{
    internal class EmptyStatement : Statement
    {
        internal override Result? Execute(Call _) => null;
    }
}
