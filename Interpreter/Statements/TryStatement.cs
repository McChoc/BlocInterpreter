using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;

namespace Bloc.Statements
{
    internal sealed record TryStatement : Statement
    {
        internal List<Statement> Try { get; set; } = null!;
        internal List<Statement> Catch { get; set; } = new();
        internal List<Statement> Finally { get; set; } = new();

        internal override Result? Execute(Call call)
        {
            Result? result;

            {
                var labels = StatementUtil.GetLabels(Try);

                call.Push();
                result = ExecuteBlock(Try, labels, call);
                call.Pop();
            }

            if (result is Throw)
            {
                var labels = StatementUtil.GetLabels(Catch);

                call.Push();
                result = ExecuteBlock(Catch, labels, call);
                call.Pop();

                if (result is not null)
                    return result;
            }

            {
                var labels = StatementUtil.GetLabels(Finally);

                call.Push();
                var finalResult = ExecuteBlock(Finally, labels, call);
                call.Pop();

                return finalResult ?? result;
            }
        }
    }
}