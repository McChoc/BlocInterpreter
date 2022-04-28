using CmmInterpretor.Data;
using CmmInterpretor.Results;
using CmmInterpretor.Tokens;
using CmmInterpretor.Values;
using System.Collections.Generic;

namespace CmmInterpretor.Statements
{
    public class WhileStatement : Statement
    {
        public bool @do;
        public bool until;
        public Token condition;
        public Token body;

        public override IResult Execute(Call call)
        {
            int loopCount = 0;

            var statements = (List<Statement>)body.value;
            var labels = GetLabels(statements);

            while (true)
            {
                bool loop;

                if (@do && loopCount == 0)
                {
                    loop = true;
                }
                else
                {
                    var result = Evaluator.Evaluate((List<Token>)condition.value, call);

                    if (result is not IValue value)
                        return result;

                    if (!value.Value().Implicit(out Bool b))
                        return new Throw("Cannot implicitly convert to bool");

                    loop = b.Value != until;
                }

                if (!loop)
                    break;

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
                        else if (result is Break)
                            break;
                        else
                            return result;
                    }
                }
                finally
                {
                    call.Pop();
                }
            }

            return new Void();
        }
    }
}
