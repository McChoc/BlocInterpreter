using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Expressions.Operators;

internal sealed record RefOperator : IExpression
{
    private readonly IExpression _operand;

    internal RefOperator(IExpression operand)
    {
        _operand = operand;
    }

    public IValue Evaluate(Call call)
    {
        var value = _operand.Evaluate(call);

        return value switch
        {
            VariablePointer pointer => new Reference(pointer),
            UnresolvedPointer pointer => new Reference(pointer.Resolve()),
            _ => throw new Throw("The operand of a reference must be a variable, a struct member or an array element")
        };
    }
}