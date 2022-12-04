using System.Collections.Generic;
using System.Linq;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;

namespace Bloc.Statements
{
    internal sealed class StatementBlock : Statement
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

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Label, Statements.Count);
        }

        public override bool Equals(object other)
        {
            return other is StatementBlock block &&
                Label == block.Label &&
                Statements.SequenceEqual(block.Statements);
        }
    }
}