using System.Collections.Generic;
using System.Linq;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Statements
{
    internal sealed class RepeatStatement : Statement
    {
        private bool @checked;
        internal override bool Checked
        {
            get => @checked;
            set => @checked = value;
        }

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
                if (Checked && i >= call.Engine.LoopLimit)
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

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Label, Checked, Expression, Statements.Count);
        }

        public override bool Equals(object other)
        {
            return other is RepeatStatement statement &&
                Label == statement.Label &&
                Checked == statement.Checked &&
                Expression.Equals(statement.Expression) &&
                Statements.SequenceEqual(statement.Statements);
        }
    }
}