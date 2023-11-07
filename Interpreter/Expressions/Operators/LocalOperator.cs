using System.Linq;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Operators;

internal sealed record LocalOperator : IExpression
{
    private readonly IExpression _operand;

    internal LocalOperator(IExpression operand)
    {
        _operand = operand;
    }

    public IValue Evaluate(Call call)
    {
        var identifier = _operand.Evaluate(call);

        return GetLocal(identifier, call);
    }

    private IValue GetLocal(IValue identifier, Call call)
    {
        if (identifier is UnresolvedPointer pointer)
        {
            if (pointer.Local is null)
                throw new Throw($"Variable {pointer.Name} was not defined in local scope");

            return new VariablePointer(pointer.Local);
        }

        if (identifier.Value is Tuple tuple)
        {
            var variables = tuple.Values
                .Select(x => GetLocal(x, call))
                .ToList();

            return new Tuple(variables);
        }

        throw new Throw("The operand must be an identifier");
    }
}