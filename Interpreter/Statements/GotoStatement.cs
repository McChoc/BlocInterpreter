using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;

namespace Bloc.Statements
{
    internal sealed record GotoStatement : Statement
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
    }
}