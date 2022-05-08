using CmmInterpretor.Data;
using CmmInterpretor.Results;
using CmmInterpretor.Tokens;
using CmmInterpretor.Values;
using System.Collections.Generic;

namespace CmmInterpretor.Statements
{
    public class IfStatement : Statement
    {
        public Token condition;
        public Token ifBody;
        public Token elseBody;

        public override IResult Execute(Call call)
        {
            var result = Evaluator.Evaluate((List<Token>)condition.value, call);

            if (result is not IValue value)
                return result;

            if (!value.Implicit(out Bool b))
                return new Throw("Cannot implicitly convert to bool");

            if (b.Value)
                return ExecuteBlock((List<Statement>)ifBody.value, call);
            else
                return ExecuteBlock((List<Statement>)elseBody.value, call);
        }
    }
}
