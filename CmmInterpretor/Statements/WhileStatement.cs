using CmmInterpretor.Memory;
using CmmInterpretor.Expressions;
using CmmInterpretor.Results;
using CmmInterpretor.Values;
using System.Collections.Generic;

namespace CmmInterpretor.Statements
{
    internal class WhileStatement : Statement
    {
        internal bool Do { get; set; }
        internal bool Until { get; set; }
        internal IExpression Condition { get; set; } = default!;
        internal List<Statement> Statements { get; set; } = default!;

        internal override Result? Execute(Call call)
        {
            int loopCount = 0;
            var labels = GetLabels(Statements);

            while (true)
            {
                bool loop;

                try
                {
                    if (Do && loopCount == 0)
                    {
                        loop = true;
                    }
                    else
                    {
                        var value = Condition.Evaluate(call);

                        if (!value.Is(out Bool? @bool))
                            return new Throw("Cannot implicitly convert to bool");

                        loop = @bool!.Value != Until;
                    }
                }
                catch (Result result)
                {
                    return result;
                }

                if (!loop)
                    break;

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
