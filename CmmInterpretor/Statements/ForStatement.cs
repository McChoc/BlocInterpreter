using CmmInterpretor.Memory;
using CmmInterpretor.Expressions;
using CmmInterpretor.Results;
using CmmInterpretor.Values;
using System.Collections.Generic;

namespace CmmInterpretor.Statements
{
    public class ForStatement : Statement
    {
        public DefStatement? Initialisation { get; set; }
        public IExpression? Condition { get; set; }
        public IExpression? Increment { get; set; }
        public List<Statement> Statements { get; set; } = default!;

        public override Result? Execute(Call call)
        {
            try
            {
                call.Push();

                Initialisation?.Execute(call);

                int loopCount = 0;

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
