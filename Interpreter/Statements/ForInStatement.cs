using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;

namespace Bloc.Statements
{
    internal class ForInStatement : Statement
    {
        internal string VariableName { get; set; } = default!;
        internal IExpression Iterable { get; set; } = default!;
        internal List<Statement> Statements { get; set; } = default!;

        internal override Result? Execute(Call call)
        {
            try
            {
                var value = Iterable.Evaluate(call).Value;

                if (value is not IIterable iterable)
                    return new Throw("You can only iterate over a range, a string or an array.");

                var loopCount = 0;
                var labels = StatementUtil.GetLabels(Statements);

                foreach (var item in iterable.Iterate())
                {
                    loopCount++;

                    if (loopCount > call.Engine.LoopLimit)
                        return new Throw("The loop limit was reached.");

                    try
                    {
                        call.Push();
                        call.Set(VariableName, item);

                        var result = ExecuteBlock(Statements, labels, call);

                        if (result is Continue)
                            continue;
                        else if (result is Break)
                            break;
                        else if (result is not null)
                            return result;
                    }
                    finally
                    {
                        call.Pop();
                    }
                }
            }
            catch (Result result)
            {
                return result;
            }

            return null;
        }
    }
}