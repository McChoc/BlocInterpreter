using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Expressions.Operators;

internal sealed record NewOperator : IExpression
{
    private readonly IExpression _operand;

    internal NewOperator(IExpression operand)
    {
        _operand = operand;
    }

    public IValue Evaluate(Call call)
    {
        var value = _operand.Evaluate(call).Value.GetOrCopy(true);
        var variable = new HeapVariable(true, value);
        var pointer = new VariablePointer(variable);

        return new Reference(pointer);
    }
}