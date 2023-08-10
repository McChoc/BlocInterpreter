using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Values.Core;
using Bloc.Variables;

namespace Bloc.Statements;

[Record]
internal sealed partial class LockStatement : Statement
{
    internal required IExpression Expression { get; init; }
    internal required Statement Statement { get; init; }

    internal override IEnumerable<IResult> Execute(Call call)
    {
        IValue? value;
        Throw? exception;

        try
        {
            value = Expression.Evaluate(call);
            exception = null;
        }
        catch (Throw e)
        {
            value = null;
            exception = e;
        }

        if (exception is not null)
        {
            yield return exception!;
            yield break;
        }

        Variable variable;

        switch (value)
        {
            case UnresolvedPointer pointer:
                variable = pointer.Resolve().Variable!;
                break;

            case VariablePointer { Variable: not null } pointer:
                variable = pointer.Variable;
                break;

            case VariablePointer:
                yield return new Throw("Invalid reference");
                yield break;

            default:
                yield return new Throw("You can only lock a variable, a struct member or an array element");
                yield break;
        }

        lock (variable)
        {
            using (call.MakeScope())
            {
                foreach (var result in ExecuteStatement(Statement, call))
                {
                    yield return result;

                    if (result is not Yield)
                        yield break;
                }
            }
        }
    }
}