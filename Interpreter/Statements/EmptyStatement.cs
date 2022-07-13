using Bloc.Memory;
using Bloc.Results;

namespace Bloc.Statements
{
    internal class EmptyStatement : Statement
    {
        internal override Result? Execute(Call _)
        {
            return null;
        }
    }
}