using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Values.Core;
using Bloc.Values.Types;
using Bloc.Variables;

namespace Bloc.Expressions.Operators;

internal sealed record ConstNewOperator : IExpression
{
    private readonly IExpression _operand;

    internal ConstNewOperator(IExpression operand)
    {
        _operand = operand;
    }

    public IValue Evaluate(Call call)
    {
        var value = _operand.Evaluate(call).Value.GetOrCopy(true);
        var variable = new HeapVariable(false, value);
        var pointer = new VariablePointer(variable);

        return new Reference(pointer);
    }
}