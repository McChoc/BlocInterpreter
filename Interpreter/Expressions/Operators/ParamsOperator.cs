using System.Linq;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Operators;

internal sealed record ParamsOperator : IExpression
{
    private readonly IExpression _operand;

    internal ParamsOperator(IExpression operand)
    {
        _operand = operand;
    }

    public IValue Evaluate(Call call)
    {
        var identifier = _operand.Evaluate(call);

        return GetParam(identifier, call);
    }

    private IValue GetParam(IValue identifier, Call call)
    {
        if (identifier is UnresolvedPointer pointer)
        {
            if (pointer.Params is null)
                throw new Throw($"Variable {pointer.Name} was not defined in params scope");

            return new VariablePointer(pointer.Params);
        }

        if (identifier.Value is Tuple tuple)
        {
            var variables = tuple.Values
                .Select(x => GetParam(x, call))
                .ToList();

            return new Tuple(variables);
        }

        throw new Throw("The operand must be an identifier");
    }
}