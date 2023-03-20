using System;
using System.Collections.Generic;
using System.Linq;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values;
using Tuple = Bloc.Values.Tuple;
using Void = Bloc.Values.Void;

namespace Bloc.Expressions.Literals;

internal sealed class TupleLiteral : IExpression
{
    private readonly List<IExpression> _expressions;

    internal TupleLiteral(List<IExpression> expressions)
    {
        _expressions = expressions;
    }

    public IValue Evaluate(Call call)
    {
        var values = new List<IValue>();

        foreach (var expression in _expressions)
        {
            var value = expression.Evaluate(call);

            if (value is Void)
                throw new Throw("'void' is not assignable");

            if (value is Pointer)
                values.Add(value);
            else
                values.Add(value.Value.GetOrCopy());
        }

        return new Tuple(values);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_expressions.Count);
    }

    public override bool Equals(object other)
    {
        return other is TupleLiteral literal &&
            _expressions.SequenceEqual(literal._expressions);
    }
}