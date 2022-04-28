using CmmInterpretor.Data;
using CmmInterpretor.Results;
using CmmInterpretor.Tokens;
using CmmInterpretor.Values;
using System.Collections.Generic;

namespace CmmInterpretor.Statements
{
    public class ForInStatement : Statement
    {
        public string variableName;
        public List<Token> iterable;
        public Token body;

        public override IResult Execute(Call call)
        {
            var result = Evaluator.Evaluate(iterable, call);

            if (result is not IValue value)
                return result;

            int loopCount = 0;

            var statements = (List<Statement>)body.value;
            var labels = GetLabels(statements);

            for (int i = 0; i < (value.Value() as IIterable).Count; i++)
            {
                loopCount++;

                if (loopCount > call.Engine.LoopLimit)
                    return new Throw("The loop limit was reached.");

                try
                {
                    call.Push();

                    call.Set(variableName, new Variable(variableName, (value.Value() as IIterable)[i], call.Scopes[^1]));

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
            }

            return new Void();
        }
    }
}
