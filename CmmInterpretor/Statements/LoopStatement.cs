using CmmInterpretor.Memory;
using CmmInterpretor.Results;
using System.Collections.Generic;

namespace CmmInterpretor.Statements
{
    internal class LoopStatement : Statement
    {
        internal List<Statement> Statements { get; set; } = default!;

        internal override Result? Execute(Call call)
        {
            int loopCount = 0;
            var labels = GetLabels(Statements);

            while (true)
            {
                loopCount++;

                if (loopCount > call.Engine.LoopLimit)
                    return new Throw("The loop limit was reached.");

                try
                {
                    call.Push();

                    var result = ExecuteBlockInLoop(Statements, labels, call);

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
