using System.Collections.Generic;
using System.Linq;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Statements
{
    internal sealed class ForInStatement : Statement
    {
        private bool @checked;
        internal override bool Checked
        {
            get => @checked;
            set => @checked = value;
        }

        internal string Name { get; set; } = null!;
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

            if (!Iter.TryImplicitCast(value!.Value, out var iter, call))
            {
                yield return new Throw("Cannot implicitly convert to iter");
                yield break;
            }

            var loopCount = 0;
            var labels = StatementUtil.GetLabels(Statements);

            foreach (var item in iter.Iterate())
            {
                if (Checked && ++loopCount > call.Engine.LoopLimit)
                {
                    yield return new Throw("The loop limit was reached.");
                    yield break;
                }

                try
                {
                    call.Push();
                    call.Set(false, Name, item);

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
            return System.HashCode.Combine(Label, Checked, Name, Expression, Statements.Count);
        }

        public override bool Equals(object other)
        {
            return other is ForInStatement statement &&
                Label == statement.Label &&
                Checked == statement.Checked &&
                Name == statement.Name &&
                Expression.Equals(statement.Expression) &&
                Statements.SequenceEqual(statement.Statements);
        }
    }
}