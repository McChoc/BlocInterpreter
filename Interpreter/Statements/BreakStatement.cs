using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;

namespace Bloc.Statements
{
    internal sealed class BreakStatement : Statement
    {
        internal override IEnumerable<Result> Execute(Call call)
        {
            yield return new Break();
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Label);
        }

        public override bool Equals(object other)
        {
            return other is BreakStatement statement &&
                Label == statement.Label;
        }
    }
}