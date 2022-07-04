using System.Collections.Generic;
using CmmInterpretor.Expressions;
using CmmInterpretor.Memory;
using CmmInterpretor.Results;
using CmmInterpretor.Values;

namespace CmmInterpretor.Statements
{
    internal class ForStatement : Statement
    {
        internal DefStatement? Initialisation { get; set; }
        internal IExpression? Condition { get; set; }
        internal IExpression? Increment { get; set; }
        internal List<Statement> Statements { get; set; } = default!;

        internal override Result? Execute(Call call)
        {
            try
            {
                call.Push();

                Initialisation?.Execute(call);

                var loopCount = 0;

                var labels = GetLabels(Statements);

                while (true)
                {
                    if (Condition is not null)
                    {
                        var value = Condition.Evaluate(call);

                        if (!value.Is(out Bool? @bool))
                            return new Throw("Cannot implicitly convert to bool");

                        if (!@bool!.Value)
                            break;
                    }

                    loopCount++;

                    if (loopCount > call.Engine.LoopLimit)
                        return new Throw("The loop limit was reached.");

                    try
                    {
                        call.Push();

                        var r = ExecuteBlockInLoop(Statements, labels, call);

                        if (r is Continue)
                            continue;
                        else if (r is Break)
                            break;
                        else if (r is not null)
                            return r;
                    }
                    finally
                    {
                        call.Pop();
                    }

                    Increment?.Evaluate(call);
                }
            }
            catch (Result result)
            {
                return result;
            }
            finally
            {
                call.Pop();
            }

            return null;
        }
    }
}