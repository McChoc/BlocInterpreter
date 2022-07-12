using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Statements
{
    internal class WhileStatement : Statement
    {
        internal bool Do { get; set; }
        internal bool Until { get; set; }
        internal IExpression Condition { get; set; } = default!;
        internal List<Statement> Statements { get; set; } = default!;

        internal override Result? Execute(Call call)
        {
            var loopCount = 0;
            var labels = StatementUtil.GetLabels(Statements);

            while (true)
            {
                try
                {
                    bool loop = true;

                    if (!Do || loopCount != 0)
                    {
                        var value = Condition.Evaluate(call).Value;

                        if (!value.Is(out Bool? @bool))
                            return new Throw("Cannot implicitly convert to bool");

                        loop = @bool!.Value != Until;
                    }

                    if (!loop)
                        break;

                    loopCount++;

                    if (loopCount > call.Engine.LoopLimit)
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
                catch (Result result)
                {
                    return result;
                }
            }

            return null;
        }
    }
}