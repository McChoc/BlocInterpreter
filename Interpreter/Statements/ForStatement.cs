using System.Collections;
using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Statements
{
    internal sealed record ForStatement : Statement, IEnumerable
    {
        internal IExpression? Initialisation { get; set; }
        internal IExpression? Condition { get; set; }
        internal IExpression? Increment { get; set; }
        internal List<Statement> Statements { get; set; }

        internal ForStatement(IExpression? initialisation, IExpression? condition, IExpression? increment)
        {
            Initialisation = initialisation;
            Condition = condition;
            Increment = increment;
            Statements = new();
        }

        internal override Result? Execute(Call call)
        {
            try
            {
                call.Push();

                Initialisation?.Evaluate(call);

                var loopCount = 0;
                var labels = StatementUtil.GetLabels(Statements);

                while (true)
                {
                    if (Condition is not null)
                    {
                        var value = Condition.Evaluate(call).Value;

                        if (!Bool.TryImplicitCast(value, out var @bool))
                            return new Throw("Cannot implicitly convert to bool");

                        if (!@bool.Value)
                            break;
                    }

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

                    Increment?.Evaluate(call);
                }
            }
            catch (Result result)
            {
                return result;
            }
            finally
            {
                call.Pop();
            }

            return null;
        }

        IEnumerator IEnumerable.GetEnumerator() => Statements.GetEnumerator();
        internal void Add(Statement statement) => Statements.Add(statement);
        internal void Add(List<Statement> statements) => Statements.AddRange(statements);
    }
}