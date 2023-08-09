using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Operators;

internal sealed record DeleteOperator : IExpression
{
    private readonly IExpression _operand;

    internal DeleteOperator(IExpression operand)
    {
        _operand = operand;
    }

    public IValue Evaluate(Call call)
    {
        var identifier = _operand.Evaluate(call);

        return Undefine(identifier);
    }

    private Value Undefine(IValue identifier)
    {
        if (identifier is Pointer pointer)
            return pointer.Delete();

        if (identifier is Tuple tuple)
        {
            var values = new List<Value>(tuple.Values.Count);

            foreach (var item in tuple.Values)
                values.Add(Undefine(item));

            return new Tuple(values);
        }

        throw new Throw("The value is not deletable");
    }
}