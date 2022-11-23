using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Statements
{
    internal sealed record RepeatStatement : Statement
    {
        internal IExpression Count { get; set; } = null!;
        internal List<Statement> Statements { get; set; } = null!;

        internal override Result? Execute(Call call)
        {
            try
            {
                var value = Count.Evaluate(call);

                if (!Number.TryImplicitCast(value.Value, out var number))
                    return new Throw("Cannot implicitly convert to number");

                var loopCount = number.GetInt();

                var labels = StatementUtil.GetLabels(Statements);

                for (var i = 0; i < loopCount; i++)
                {
                    if (i >= call.Engine.LoopLimit)
                        return new Throw("The loop limit was reached.");

                    try
                    {
                        call.Push();

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