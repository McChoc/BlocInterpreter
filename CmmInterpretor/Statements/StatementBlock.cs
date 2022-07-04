using System.Collections.Generic;
using CmmInterpretor.Memory;
using CmmInterpretor.Results;

namespace CmmInterpretor.Statements
{
    internal class StatementBlock : Statement
    {
        internal List<Statement> Statements { get; set; } = default!;

        internal override Result? Execute(Call call)
        {
            return ExecuteBlock(Statements, call);
        }
    }
}