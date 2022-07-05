using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Variables;

namespace Bloc.Statements
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