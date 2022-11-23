using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Statements
{
    internal sealed record ForInStatement : Statement
    {
        internal string Name { get; set; } = null!;
        internal IExpression Expression { get; set; } = null!;
        internal List<Statement> Statements { get; set; } = null!;

        internal override Result? Execute(Call call)
        {
            try
            {
                var value = Expression.Evaluate(call).Value;

                if (!Iter.TryImplicitCast(value, out var iter, call))
                    return new Throw("Cannot implicitly convert to iter");

                var loopCount = 0;
                var labels = StatementUtil.GetLabels(Statements);

                foreach (var item in iter.Iterate())
                {
                    loopCount++;

                    if (loopCount > call.Engine.LoopLimit)
                        return new Throw("The loop limit was reached.");

                    try
                    {
                        call.Push();
                        call.Set(Name, item);

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