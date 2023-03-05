using System.Linq;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Operators;

internal sealed record LetOperator : IExpression
{
    private readonly IExpression _operand;

    internal LetOperator(IExpression operand)
    {
        _operand = operand;
    }

    public IValue Evaluate(Call call)
    {
        var identifier = _operand.Evaluate(call);

        return Define(identifier, call);
    }

    private IValue Define(IValue identifier, Call call)
    {
        if (identifier is UnresolvedPointer pointer)
            return pointer.Define(false, true, Null.Value, call);

        if (identifier.Value is Tuple tuple)
        {
            var variables = tuple.Values
                .Select(x => Define(x, call))
                .ToList();

            return new Tuple(variables);
        }

        throw new Throw("The operand must be an identifier");
    }
}