using CmmInterpretor.Data;
using CmmInterpretor.Results;
using CmmInterpretor.Tokens;
using CmmInterpretor.Values;
using System.Collections.Generic;

namespace CmmInterpretor.Statements
{
    public class IfStatement : Statement
    {
        public List<Token> conditions = new();
        public List<Token> bodies = new();

        public override IResult Execute(Call call)
        {
            for (int i = 0; i < bodies.Count; i++)
            {
                if (i == conditions.Count)
                    return ExecuteBlock((List<Statement>)bodies[i].value, call);

                var result = Evaluator.Evaluate((List<Token>)conditions[i].value, call);

                if (result is not IValue value)
                    return result;

                if (!value.Value().Implicit(out Bool b))
                    return new Throw("Cannot implicitly convert to bool");

                if (b.Value)
                    return ExecuteBlock((List<Statement>)bodies[i].value, call);
            }

            return new Void();
        }
    }
}
