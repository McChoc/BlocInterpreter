﻿using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Statements
{
    internal class RepeatStatement : Statement
    {
        internal IExpression Count { get; set; } = default!;
        internal List<Statement> Statements { get; set; } = default!;

        internal override Result? Execute(Call call)
        {
            try
            {
                var value = Count.Evaluate(call);

                if (!value.Value.Is(out Number? number))
                    return new Throw("Cannot implicitly convert to number");

                var loopCount = number!.ToInt();
                var labels = GetLabels(Statements);

                for (var i = 0; i < loopCount; i++)
                {
                    if (i >= call.Engine.LoopLimit)
                        return new Throw("The loop limit was reached.");

                    try
                    {
                        call.Push();

                        var r = ExecuteBlockInLoop(Statements, labels, call);

                        if (r is Continue)
                            continue;
                        else if (r is Break)
                            break;
                        else if (r is not null)
                            return r;
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