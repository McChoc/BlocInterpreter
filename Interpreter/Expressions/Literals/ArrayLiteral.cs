using System.Collections.Generic;
using System.Linq;
using Bloc.Expressions.Literals.ArrayElements;
using Bloc.Memory;
using Bloc.Utils.Attributes;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Literals;

[Record]
internal sealed partial class ArrayLiteral : IExpression
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
}