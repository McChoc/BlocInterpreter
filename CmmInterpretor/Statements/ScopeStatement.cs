using CmmInterpretor.Memory;
using CmmInterpretor.Results;
using System.Collections.Generic;

namespace CmmInterpretor.Statements
{
    public class ScopeStatement : Statement
    {
        public List<Statement> Statements { get; set; } = default!;

        public override Result? Execute(Call call) => ExecuteBlock(Statements, call);
    }
}
