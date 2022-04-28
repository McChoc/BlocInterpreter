using CmmInterpretor.Data;
using CmmInterpretor.Results;
using CmmInterpretor.Tokens;
using CmmInterpretor.Values;
using System.Collections.Generic;

namespace CmmInterpretor.Statements
{
    public class ForStatement : Statement
    {
        public List<List<Token>> initialisation;
        public List<Token> condition;
        public List<Token> increment;
        public Token body;

        public override IResult Execute(Call call)
        {
            call.Push();

            new DefStatement() { definitions = initialisation }.Execute(call);

            int loopCount = 0;

            var statements = (List<Statement>)body.value;
            var labels = GetLabels(statements);

            while (true)
            {
                var result = Evaluator.Evaluate(condition, call);

                if (result is not IValue value)
                    return result;

                if (!value.Value().Implicit(out Bool b))
                    return new Throw("Cannot implicitly convert to bool");

                if (!b.Value)
                    break;

                loopCount++;

                if (loopCount > call.Engine.LoopLimit)
                    return new Throw("The loop limit was reached.");

                try
                {
                    call.Push();

                    var r = ExecuteBlockInLoop(statements, labels, call);

                    if (r is not IValue)
                    {
                        if (r is Continue)
                            continue;
                        else if (r is Break)
                            break;
                        else
                            return r;
                    }
                }
                finally
                {
                    call.Pop();
                }

                Evaluator.Evaluate(increment, call);
            }

            call.Pop();

            return new Void();
        }
    }
}
