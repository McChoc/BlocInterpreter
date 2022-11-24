using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;

namespace Bloc.Statements
{
    internal sealed record ContinueStatement : Statement
    {
        internal override IEnumerable<Result> Execute(Call call)
        {
            yield return new Continue();
        }
    }
}