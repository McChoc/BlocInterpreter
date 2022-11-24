using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;

namespace Bloc.Statements
{
    internal sealed record LoopStatement : Statement
    {
        internal List<Statement> Statements { get; set; } = null!;

        internal override IEnumerable<Result> Execute(Call call)
        {
            var loopCount = 0;
            var labels = StatementUtil.GetLabels(Statements);

            while (true)
            {
                if (++loopCount > call.Engine.LoopLimit)
                {
                    yield return new Throw("The loop limit was reached.");
                    yield break;
                }

                try
                {
                    call.Push();

                    foreach (var result in ExecuteBlock(Statements, labels, call))
                    {
                        switch (result)
                        {
                            case Continue:
                                goto Continue;

                            case Break:
                                goto Break;

                            case Yield:
                                yield return result;
                                break;

                            default:
                                yield return result;
                                yield break;
                        }
                    }
                }
                finally
                {
                    call.Pop();
                }

            Continue:;
            }

        Break:;
        }
    }
}