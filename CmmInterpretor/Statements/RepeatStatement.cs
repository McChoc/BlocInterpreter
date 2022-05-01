using CmmInterpretor.Data;
using CmmInterpretor.Results;
using CmmInterpretor.Tokens;
using CmmInterpretor.Values;
using System.Collections.Generic;

namespace CmmInterpretor.Statements
{
    public class RepeatStatement : Statement
    {
        public Token count;
        public Token body;

        public override IResult Execute(Call call)
        {
            var result = Evaluator.Evaluate((List<Token>)count.value, call);

            if (result is not IValue value)
                return result;

            if (!value.Implicit(out Number num))
                return new Throw("Cannot implicitly convert to number");

            int loopCount = num.ToInt();

            var statements = (List<Statement>)body.value;
            var labels = GetLabels(statements);

            for (int i = 0; i < loopCount; i++)
            {
                if (i >= call.Engine.LoopLimit)
                    return new Throw("The loop limit was reached.");

                try
                {
                    call.Push();

                    var r = ExecuteBlockInLoop(statements, labels, call);

                    if (r is not IValue)
                    {
                        if (r is Continue)
                            continue;

                        if (r is Break)
                            break;
                        
                        return r;
                    }
                }
                finally
                {
                    call.Pop();
                }
            }

            return Void.Value;
        }
    }
}
