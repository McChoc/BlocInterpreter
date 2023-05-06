using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Expressions.Operators;

internal sealed record AwaitOperator : IExpression
{
    private readonly IExpression _operand;

    internal AwaitOperator(IExpression operand)
    {
        _operand = operand;
    }

    public IValue Evaluate(Call call)
    {
        var value = _operand.Evaluate(call).Value;

        value = ReferenceHelper.Resolve(value, call.Engine.Options.HopLimit).Value;

        if (value is Task task)
            return task.Await();

        throw new Throw($"Cannot apply operator 'await' on type {value.GetTypeName()}");
    }
}