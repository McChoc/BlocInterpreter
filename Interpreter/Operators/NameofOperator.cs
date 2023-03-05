using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Operators;

internal sealed record NameofOperator : IExpression
{
    private readonly IExpression _operand;

    internal NameofOperator(IExpression operand)
    {
        _operand = operand;
    }

    public IValue Evaluate(Call call)
    {
        var value = _operand.Evaluate(call);

        if (value is VariablePointer pointer)
        {
            if (pointer.Variable is StackVariable variable)
                return new String(variable.Name);

            if (pointer.Variable is StructVariable member)
                return new String(member.Name);
        }

        throw new Throw("The expression does not have a name");
    }
}