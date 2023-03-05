using System.Linq;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Utils.Helpers;

internal delegate Value UnaryOperation(Value value);

internal delegate Value BinaryOperation(Value left, Value right);

internal static class OperatorHelper
{
    internal static Value RecursivelyCall(Value value, UnaryOperation operation, Call call)
    {
        value = ReferenceHelper.Resolve(value, call.Engine.HopLimit).Value;

        if (value is Tuple tuple)
            return new Tuple(tuple.Values
                .Select(x => RecursivelyCall(x.Value, operation, call))
                .ToList());

        return operation(value);
    }

    internal static Value RecursivelyCall(Value left, Value right, BinaryOperation operation, Call call)
    {
        left = ReferenceHelper.Resolve(left, call.Engine.HopLimit).Value;
        right = ReferenceHelper.Resolve(right, call.Engine.HopLimit).Value;

        switch (left, right)
        {
            case (Tuple leftTuple, Tuple rightTuple):
                if (leftTuple.Values.Count != rightTuple.Values.Count)
                    throw new Throw("Miss mathch number of elements inside the tuples");

                return new Tuple(leftTuple.Values
                    .Zip(rightTuple.Values, (a, b) => RecursivelyCall(a.Value, b.Value, operation, call))
                    .ToList());

            case (Tuple tuple, _):
                return new Tuple(tuple.Values
                    .Select(x => RecursivelyCall(x.Value, right, operation, call))
                    .ToList());

            case (_, Tuple tuple):
                return new Tuple(tuple.Values
                    .Select(x => RecursivelyCall(left, x.Value, operation, call))
                    .ToList());

            default:
                return operation(left, right);
        }
    }
}