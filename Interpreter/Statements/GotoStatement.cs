using Bloc.Memory;
using Bloc.Results;

namespace Bloc.Statements
{
    internal class GotoStatement : Statement
    {
        internal GotoStatement(string label)
        {
            Label = label;
        }

        internal new string Label { get; }

        internal override Result Execute(Call call)
        {
            return new Goto(Label);
        }
    }
}