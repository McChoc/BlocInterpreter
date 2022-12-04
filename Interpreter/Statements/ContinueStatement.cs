using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;

namespace Bloc.Statements
{
    internal sealed class ContinueStatement : Statement
    {
        internal override IEnumerable<Result> Execute(Call call)
        {
            yield return new Continue();
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Label);
        }

        public override bool Equals(object other)
        {
            return other is ContinueStatement statement &&
                Label == statement.Label;
        }
    }
}