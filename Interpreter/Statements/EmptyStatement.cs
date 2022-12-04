using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;

namespace Bloc.Statements
{
    internal sealed class EmptyStatement : Statement
    {
        internal override IEnumerable<Result> Execute(Call _)
        {
            yield break;
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Label);
        }

        public override bool Equals(object other)
        {
            return other is EmptyStatement statement &&
                Label == statement.Label;
        }
    }
}