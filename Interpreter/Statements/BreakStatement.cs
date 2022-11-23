using Bloc.Memory;
using Bloc.Results;

namespace Bloc.Statements
{
    internal sealed record BreakStatement : Statement
    {
        internal override Result Execute(Call call)
        {
            return new Break();
        }
    }
}