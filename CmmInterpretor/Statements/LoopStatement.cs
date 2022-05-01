using CmmInterpretor.Data;
using CmmInterpretor.Results;
using CmmInterpretor.Tokens;
using CmmInterpretor.Values;
using System.Collections.Generic;

namespace CmmInterpretor.Statements
{
    public class LoopStatement : Statement
    {
        public Token body;

        public override IResult Execute(Call call)
        {
            int loopCount = 0;

            var statements = (List<Statement>)body.value;
            var labels = GetLabels(statements);

            while (true)
            {
                loopCount++;

                if (loopCount > call.Engine.LoopLimit)
                    return new Throw("The loop limit was reached.");

                try
                {
                    call.Push();

                    var result = ExecuteBlockInLoop(statements, labels, call);

                    if (result is not IValue)
                    {
                        if (result is Continue)
                            continue;

                        if (result is Break)
                            break;
                        
                        return result;
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
