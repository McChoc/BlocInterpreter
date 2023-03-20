using System.Linq;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Expressions.Operators;

internal sealed record NonlocalOperator : IExpression
{
    private readonly IExpression _operand;

    internal NonlocalOperator(IExpression operand)
    {
        _operand = operand;
    }

    public IValue Evaluate(Call call)
    {
        var identifier = _operand.Evaluate(call);

        return GetCapture(identifier, call);
    }

    private IValue GetCapture(IValue identifier, Call call)
    {
        if (identifier is UnresolvedPointer pointer)
        {
            if (pointer.NonLocal is null)
                throw new Throw($"Variable {pointer.Name} was not defined in non-local scope");

            return new VariablePointer(pointer.NonLocal);
        }

        if (identifier.Value is Tuple tuple)
        {
            var variables = tuple.Values
                .Select(x => GetCapture(x, call))
                .ToList();

            return new Tuple(variables);
        }

        throw new Throw("The operand must be an identifier");
    }
}