using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Literals.ArrayElements;

internal sealed record UnpackedElement : IElement
{
    private readonly IExpression _expression;

    public UnpackedElement(IExpression expression)
    {
        _expression = expression;
    }

    public IEnumerable<Value> GetElements(Call call)
    {
        var value = _expression.Evaluate(call).Value.GetOrCopy();

        if (value is not Array array)
            throw new Throw("Only an array can be unpacked using the array unpack syntax");

        foreach (var val in array.Values)
            yield return val.Value;
    }
}