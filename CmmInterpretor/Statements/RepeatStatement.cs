using CmmInterpretor.Memory;
using CmmInterpretor.Expressions;
using CmmInterpretor.Results;
using CmmInterpretor.Values;
using System.Collections.Generic;

namespace CmmInterpretor.Statements
{
    public class RepeatStatement : Statement
    {
        public IExpression Count { get; set; } = default!;
        public List<Statement> Statements { get; set; } = default!;

        public override Result? Execute(Call call)
        {
            try
            {
                var value = Count.Evaluate(call);

                if (!value.Is(out Number? number))
                    return new Throw("Cannot implicitly convert to number");

                int loopCount = number!.ToInt();
                var labels = GetLabels(Statements);

                for (int i = 0; i < loopCount; i++)
                {
                    if (i >= call.Engine.LoopLimit)
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
                }
            }
            catch (Result result)
            {
                return result;
            }
            
            return null;
        }
    }
}
