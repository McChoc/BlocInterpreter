using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;

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

                if (value is not Pointer pointer)
                    return new Throw("You can only lock a variable");

                if (pointer is UndefinedPointer undefined)
                    return new Throw($"Variable {undefined.Name} was not defined in scope");

                if (pointer is SlicePointer)
                    return new Throw("You cannot lock a slice");

                var variable = (VariablePointer)pointer;

                if (variable.Variable is null)
                    return new Throw("Invalid reference");

                var labels = StatementUtil.GetLabels(Statements);

                lock (variable.Variable)
                {
                    call.Push();
                    var result = ExecuteBlock(Statements, labels, call);
                    call.Pop();

                    return result;
                }
            }
            catch (Result result)
            {
                return result;
            }
        }
    }
}