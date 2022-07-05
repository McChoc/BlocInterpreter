using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;

namespace Bloc.Statements
{
    internal class TryStatement : Statement
    {
        internal List<Statement> Try { get; set; } = default!;
        internal List<Statement> Catch { get; set; } = new();
        internal List<Statement> Finally { get; set; } = new();

        internal override Result? Execute(Call call)
        {
            var result = ExecuteBlock(Try, call);

            if (result is Throw)
            {
                result = null;

                var catchResult = ExecuteBlock(Catch, call);

                if (catchResult is not null)
                    return catchResult;
            }

            var finallyResult = ExecuteBlock(Finally, call);

            if (finallyResult is not null)
                return finallyResult;

            return result;
        }
    }
}