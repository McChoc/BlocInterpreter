using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;

namespace Bloc.Statements
{
    internal class LoopStatement : Statement
    {
        internal List<Statement> Statements { get; set; } = default!;

        internal override Result? Execute(Call call)
        {
            var loopCount = 0;
            var labels = StatementUtil.GetLabels(Statements);

            while (true)
            {
                loopCount++;

                if (loopCount > call.Engine.LoopLimit)
                    return new Throw("The loop limit was reached.");

                try
                {
                    call.Push();

                    var result = ExecuteBlock(Statements, labels, call);

                    if (result is Continue)
                        continue;
                    else if (result is Break)
                        break;
                    else if (result is not null)
                        return result;
                }
                finally
                {
                    call.Pop();
                }
            }

            return null;
        }
    }
}