using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;

namespace Bloc.Statements
{
    internal class StatementBlock : Statement
    {
        internal List<Statement> Statements { get; set; } = default!;

        internal override Result? Execute(Call call)
        {
            var labels = StatementUtil.GetLabels(Statements);

            call.Push();
            var result = ExecuteBlock(Statements, labels, call);
            call.Pop();

            return result;
        }
    }
}