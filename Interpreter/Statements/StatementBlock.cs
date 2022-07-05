using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;

namespace Bloc.Statements
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