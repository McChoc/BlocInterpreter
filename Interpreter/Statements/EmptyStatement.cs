using Bloc.Memory;
using Bloc.Results;

namespace Bloc.Statements
{
    internal sealed record EmptyStatement : Statement
    {
        internal override Result? Execute(Call _)
        {
            return null;
        }
    }
}