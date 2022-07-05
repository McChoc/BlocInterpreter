using Bloc.Memory;
using Bloc.Results;

namespace Bloc.Statements
{
    internal class ContinueStatement : Statement
    {
        internal override Result Execute(Call call)
        {
            return new Continue();
        }
    }
}