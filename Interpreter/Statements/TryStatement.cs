using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;

namespace Bloc.Statements
{
    internal sealed record TryStatement : Statement
    {
        internal List<Statement> Try { get; set; } = new();
        internal List<Statement> Catch { get; set; } = new();
        internal List<Statement> Finally { get; set; } = new();

        internal override IEnumerable<Result> Execute(Call call)
        {
            Result? result = null;

            try
            {
                var labels = StatementUtil.GetLabels(Try);

                call.Push();

                foreach (var r in ExecuteBlock(Try, labels, call))
                {
                    if (r is not Yield)
                    {
                        result = r;
                        break;
                    }

                    yield return r;
                }
            }
            finally
            {
                call.Pop();
            }

            if (result is Throw)
            {
                result = null;

                try
                {
                    var labels = StatementUtil.GetLabels(Catch);

                    call.Push();

                    foreach (var r in ExecuteBlock(Catch, labels, call))
                    {
                        if (r is not Yield)
                        {
                            result = r;
                            break;
                        }

                        yield return r;
                    }
                }
                finally
                {
                    call.Pop();
                }
            }

            try
            {
                var labels = StatementUtil.GetLabels(Finally);

                call.Push();

                foreach (var r in ExecuteBlock(Finally, labels, call))
                {
                    if (r is not Yield)
                    {
                        result = r;
                        break;
                    }

                    yield return r;
                }
            }
            finally
            {
                call.Pop();
            }

            if (result is not null)
                yield return result;
        }
    }
}