using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;

namespace Bloc.Statements
{
    internal sealed record StatementBlock : Statement
    {
        internal List<Statement> Statements { get; set; } = null!;

        internal override IEnumerable<Result> Execute(Call call)
        {
            var labels = StatementUtil.GetLabels(Statements);

            try
            {
                call.Push();

                foreach (var result in ExecuteBlock(Statements, labels, call))
                {
                    yield return result;

                    if (result is not Yield)
                        yield break;
                }
            }
            finally
            {
                call.Pop();
            }
        }
    }
}