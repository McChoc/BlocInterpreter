using CmmInterpretor.Expressions;
using CmmInterpretor.Memory;
using CmmInterpretor.Results;
using System.Collections.Generic;

namespace CmmInterpretor.Statements
{
    public class LockStatement : Statement
    {
        public IExpression Expression { get; set; } = default!;
        public List<Statement> Statements { get; set; } = default!;

        public override Result? Execute(Call call)
        {
            try
            {
                var value = Expression.Evaluate(call);

                lock (value)
                {
                    return ExecuteBlock(Statements, call);
                }
            }
            catch (Result result)
            {
                return result;
            }
        }
    }
}
