using CmmInterpretor.Memory;
using CmmInterpretor.Results;

namespace CmmInterpretor.Statements
{
    public class EmptyStatement : Statement
    {
        public override Result? Execute(Call _) => null;
    }
}
