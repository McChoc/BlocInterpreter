using System.Collections.Generic;
using CmmInterpretor.Expressions;
using CmmInterpretor.Memory;
using CmmInterpretor.Results;
using CmmInterpretor.Variables;

namespace CmmInterpretor.Statements
{
    internal class LockStatement : Statement
    {
        internal IExpression Expression { get; set; } = default!;
        internal List<Statement> Statements { get; set; } = default!;

        internal override Result? Execute(Call call)
        {
            try
            {
                var value = Expression.Evaluate(call);

                if (value is not Variable)
                    return new Throw("You can only lock a variable");

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