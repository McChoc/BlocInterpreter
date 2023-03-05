using System.Linq;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Operators;

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
        switch (left)
        {
            case Pointer pointer:
                return pointer.Set(right.Value);

            case Tuple tuple:
                if (right.Value is not Tuple rightTuple)
                    return new Tuple(tuple.Values.Select(x => Assign(x, right)).ToList());

                if (tuple.Values.Count == rightTuple.Values.Count)
                    return new Tuple(tuple.Values.Zip(rightTuple.Values, Assign).ToList());

                throw new Throw("Miss mathch number of elements inside the tuples");

            default:
                throw new Throw("The left part of an assignment must be assignable");
        };
    }
}