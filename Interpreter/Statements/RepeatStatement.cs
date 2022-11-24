using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Statements
{
    internal sealed record RepeatStatement : Statement
    {
        internal IExpression Expression { get; set; } = null!;
        internal List<Statement> Statements { get; set; } = null!;

        internal override IEnumerable<Result> Execute(Call call)
        {
            var (value, exception) = EvaluateExpression(Expression, call);

            if (exception is not null)
            {
                yield return exception;
                yield break;
            }

            if (!Number.TryImplicitCast(value!.Value, out var number))
            {
                yield return new Throw("Cannot implicitly convert to number");
                yield break;
            }

            var loopCount = number.GetInt();

            var labels = StatementUtil.GetLabels(Statements);

            for (var i = 0; i < loopCount; i++)
            {
                if (i >= call.Engine.LoopLimit)
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