using System.Collections.Generic;
using System.Linq;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;

namespace Bloc.Statements
{
    internal sealed class LoopStatement : Statement
    {
        private bool @checked;
        internal override bool Checked
        {
            get => @checked;
            set => @checked = value;
        }

        internal List<Statement> Statements { get; set; } = null!;

        internal override IEnumerable<Result> Execute(Call call)
        {
            var loopCount = 0;
            var labels = StatementUtil.GetLabels(Statements);

            while (true)
            {
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
            return System.HashCode.Combine(Label, Checked, Statements.Count);
        }

        public override bool Equals(object other)
        {
            return other is LoopStatement statement &&
                Label == statement.Label &&
                Checked == statement.Checked &&
                Statements.SequenceEqual(statement.Statements);
        }
    }
}