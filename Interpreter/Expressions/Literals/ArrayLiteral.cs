using System;
using System.Collections.Generic;
using System.Linq;
using Bloc.Expressions.Literals.ArrayElements;
using Bloc.Memory;
using Bloc.Values.Core;
using Array = Bloc.Values.Types.Array;

namespace Bloc.Expressions.Literals;

internal sealed class ArrayLiteral : IExpression
{
    private readonly List<IElement> _elements;

    internal ArrayLiteral(List<IElement> elements)
    {
        _elements = elements;
    }

    public IValue Evaluate(Call call)
    {
        var values = _elements
            .SelectMany(x => x.GetElements(call))
            .ToList();

        return new Array(values);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_elements.Count);
    }

    public override bool Equals(object other)
    {
        return other is ArrayLiteral literal &&
            _elements.SequenceEqual(literal._elements);
    }
}