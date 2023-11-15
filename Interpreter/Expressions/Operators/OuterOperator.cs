using System.Linq;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Operators;

internal sealed record OuterOperator : IExpression
{
    private readonly IExpression _operand;

    internal OuterOperator(IExpression operand)
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
            if (pointer.Outer is null)
                throw new Throw($"Variable {pointer.Name} was not defined in outer scope");

            return new VariablePointer(pointer.Outer);
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