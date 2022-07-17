using System.Linq;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Utils
{
    internal delegate Value UnaryOperation(Value value);

    internal delegate Value BinaryOperation(Value left, Value right);

    internal static class OperatorUtil
    {
        internal static Value RecursivelyCall(Value value, UnaryOperation operation, Call call)
        {
            value = ReferenceUtil.Dereference(value, call.Engine).Value;

            if (value is Tuple tuple)
                return new Tuple(tuple.Values.Select(x => RecursivelyCall(x.Value, operation, call))
                    .ToList<IPointer>());

            return operation(value);
        }

        internal static Value RecursivelyCall(Value left, Value right, BinaryOperation operation, Call call)
        {
            left = ReferenceUtil.Dereference(left, call.Engine).Value;
            right = ReferenceUtil.Dereference(right, call.Engine).Value;

            if (left is Tuple leftTuple && right is Tuple rightTuple)
            {
                if (leftTuple.Values.Count != rightTuple.Values.Count)
                    throw new Throw("Miss mathch number of elements inside the tuples");

                return new Tuple(leftTuple.Values
                    .Zip(rightTuple.Values, (a, b) => RecursivelyCall(a.Value, b.Value, operation, call))
                    .ToList<IPointer>());
            }

            {
                if (left is Tuple tuple)
                    return new Tuple(tuple.Values.Select(x => RecursivelyCall(x.Value, right, operation, call))
                        .ToList<IPointer>());
            }

            {
                if (right is Tuple tuple)
                    return new Tuple(tuple.Values.Select(x => RecursivelyCall(left, x.Value, operation, call))
                        .ToList<IPointer>());
            }

            return operation(left, right);
        }
    }
}