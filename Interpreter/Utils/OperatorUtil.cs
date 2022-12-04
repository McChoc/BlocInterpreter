using System.Linq;
using Bloc.Memory;
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
            value = ReferenceUtil.Dereference(value, call.Engine.HopLimit).Value;

            if (value is Tuple tuple)
                return new Tuple(tuple.Variables
                    .Select(x => RecursivelyCall(x.Value, operation, call))
                    .ToList());

            return operation(value);
        }

        internal static Value RecursivelyCall(Value left, Value right, BinaryOperation operation, Call call)
        {
            left = ReferenceUtil.Dereference(left, call.Engine.HopLimit).Value;
            right = ReferenceUtil.Dereference(right, call.Engine.HopLimit).Value;

            if (left is Tuple leftTuple && right is Tuple rightTuple)
            {
                if (leftTuple.Variables.Count != rightTuple.Variables.Count)
                    throw new Throw("Miss mathch number of elements inside the tuples");

                return new Tuple(leftTuple.Variables
                    .Zip(rightTuple.Variables, (a, b) => RecursivelyCall(a.Value, b.Value, operation, call))
                    .ToList());
            }

            {
                if (left is Tuple tuple)
                    return new Tuple(tuple.Variables
                        .Select(x => RecursivelyCall(x.Value, right, operation, call))
                        .ToList());
            }

            {
                if (right is Tuple tuple)
                    return new Tuple(tuple.Variables
                        .Select(x => RecursivelyCall(left, x.Value, operation, call))
                        .ToList());
            }

            return operation(left, right);
        }
    }
}