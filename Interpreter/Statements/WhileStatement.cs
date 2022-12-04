using System.Collections.Generic;
using System.Linq;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Statements
{
    internal sealed class WhileStatement : Statement
    {
        private bool @checked;
        internal override bool Checked
        {
            get => @checked;
            set => @checked = value;
        }

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

                if (Checked && ++loopCount > call.Engine.LoopLimit)
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
            return System.HashCode.Combine(Label, Checked, Do, Until, Expression, Statements.Count);
        }

        public override bool Equals(object other)
        {
            return other is WhileStatement statement &&
                Label == statement.Label &&
                Checked == statement.Checked &&
                Do == statement.Do &&
                Until == statement.Until &&
                Expression.Equals(statement.Expression) &&
                Statements.SequenceEqual(statement.Statements);
        }
    }
}