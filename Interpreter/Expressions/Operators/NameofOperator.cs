using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values.Core;
using Bloc.Values.Types;
using Bloc.Variables;

namespace Bloc.Expressions.Operators;

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

        var pointer = value switch
        {
            UnresolvedPointer unresolvedPointer => unresolvedPointer.Resolve(),
            VariablePointer variablePointer => variablePointer,
            _ => throw new Throw("The expression does not have a name")
        };

        var name = pointer.Variable switch
        {
            StackVariable variable => variable.Name,
            StructVariable variable => variable.Name,
            _ => throw new Throw("The expression does not have a name")
        };

        return new String(name);
    }
}