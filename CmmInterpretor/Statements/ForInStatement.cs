using CmmInterpretor.Data;
using CmmInterpretor.Results;
using CmmInterpretor.Tokens;
using CmmInterpretor.Values;
using CmmInterpretor.Variables;
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

            if (value.Value is not IIterable iter)
                return new Throw("You can only iterate over a range, a string or an array.");

            foreach (var item in iter.Iterate())
            {
                loopCount++;

                if (loopCount > call.Engine.LoopLimit)
                    return new Throw("The loop limit was reached.");

                try
                {
                    call.Push();

                    call.Set(variableName, new StackVariable(item, variableName, call.Scopes[^1]));

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
