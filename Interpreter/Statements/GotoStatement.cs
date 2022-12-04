using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;

namespace Bloc.Statements
{
    internal sealed class GotoStatement : Statement
    {
        internal GotoStatement(string label)
        {
            Label = label;
        }

        internal new string Label { get; }

        internal override IEnumerable<Result> Execute(Call call)
        {
            yield return new Goto(Label);
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(base.Label, Label);
        }

        public override bool Equals(object other)
        {
            return other is GotoStatement statement &&
                base.Label == ((Statement)statement).Label &&
                Label == statement.Label;
        }
    }
}