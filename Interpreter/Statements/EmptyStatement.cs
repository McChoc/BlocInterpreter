using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;

namespace Bloc.Statements
{
    internal sealed record EmptyStatement : Statement
    {
        internal override IEnumerable<Result> Execute(Call _)
        {
            yield break;
        }
    }
}