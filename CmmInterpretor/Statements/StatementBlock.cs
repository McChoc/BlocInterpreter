using CmmInterpretor.Memory;
using CmmInterpretor.Results;
using System.Collections.Generic;

namespace CmmInterpretor.Statements
{
    internal class StatementBlock : Statement
    {
        internal List<Statement> Statements { get; set; } = default!;

        internal override Result? Execute(Call call) => ExecuteBlock(Statements, call);
    }
}
