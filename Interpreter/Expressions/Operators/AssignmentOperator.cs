using System.Linq;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Operators;

internal sealed record AssignmentOperator : IExpression
{
    private readonly IExpression _left;
    private readonly IExpression _right;

    internal AssignmentOperator(IExpression left, IExpression right)
    {
        _left = left;
        _right = right;
    }

    public IValue Evaluate(Call call)
    {
        var left = _left.Evaluate(call);
        var right = _right.Evaluate(call);

        return Assign(left, right);
    }

    private static Value Assign(IValue left, IValue right)
    {
        var value = right.Value.GetOrCopy();

        switch (left)
        {
            case Pointer pointer:
                return pointer.Set(value);

            case Tuple { Assignable: true } tuple:
                if (value is not Tuple rightTuple)
                    return new Tuple(tuple.Values.Select(x => Assign(x, value)).ToList());

                if (tuple.Values.Count == rightTuple.Values.Count)
                    return new Tuple(tuple.Values.Zip(rightTuple.Values, Assign).ToList());

                throw new Throw("Miss mathch number of elements inside the tuples");

            default:
                throw new Throw("The left part of an assignment must be assignable");
        };
    }
}