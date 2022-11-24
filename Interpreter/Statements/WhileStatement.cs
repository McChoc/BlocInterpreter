using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Statements
{
    internal sealed record WhileStatement : Statement
    {
        internal bool Do { get; set; }
        internal bool Until { get; set; }
        internal IExpression Expression { get; set; } = null!;
        internal List<Statement> Statements { get; set; } = null!;

        internal override IEnumerable<Result> Execute(Call call)
        {
            var loopCount = 0;
            var labels = StatementUtil.GetLabels(Statements);

            while (true)
            {
                if (!Do || loopCount != 0)
                {
                    var (value, exception) = EvaluateExpression(Expression, call);

                    if (exception is not null)
                    {
                        yield return exception;
                        yield break;
                    }

                    if (!Bool.TryImplicitCast(value!.Value, out var @bool))
                    {
                        yield return new Throw("Cannot implicitly convert to bool");
                        yield break;
                    }

                    if (@bool.Value == Until)
                        break;
                }

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