using System.Linq;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Utils
{
    internal delegate IPointer UnaryOperation(IPointer value);

    internal delegate IPointer BinaryOperation(IPointer left, IPointer right);

    internal static class TupleUtil
    {
        internal static IPointer RecursivelyCall(IPointer value, UnaryOperation operation)
        {
            if (value.Value is Tuple tuple)
                return new Tuple(tuple.Values.Select(x => RecursivelyCall(x, operation)).ToList());

            return operation(value);
        }

        internal static IPointer RecursivelyCall(IPointer left, IPointer right, BinaryOperation operation)
        {
            if (left.Value is Tuple leftTuple && right.Value is Tuple rightTuple)
            {
                if (leftTuple.Values.Count != rightTuple.Values.Count)
                    throw new Throw("Miss mathch number of elements inside the tuples");

                return new Tuple(leftTuple.Values.Zip(rightTuple.Values, (a, b) => RecursivelyCall(a, b, operation)).ToList());
            }

            {
                if (left.Value is Tuple tuple)
                    return new Tuple(tuple.Values.Select(x => RecursivelyCall(x, right, operation)).ToList());
            }

            {
                if (right.Value is Tuple tuple)
                    return new Tuple(tuple.Values.Select(x => RecursivelyCall(left, x, operation)).ToList());
            }

            return operation(left, right);
        }

        internal static IPointer RecursivelyAssign(IPointer left, IPointer right)
        {
            if (left is Tuple leftTuple && right.Value is Tuple rightTuple)
            {
                if (leftTuple.Values.Count != rightTuple.Values.Count)
                    throw new Throw("Miss mathch number of elements inside the tuples");

                return new Tuple(leftTuple.Values.Zip(rightTuple.Values, (a, b) => RecursivelyAssign(a, b)).ToList());
            }

            if (left is Tuple tuple)
                return new Tuple(tuple.Values.Select(x => RecursivelyAssign(x, right)).ToList());

            return Assign(left, right);
        }

        internal static IPointer RecursivelyCompoundAssign(IPointer left, IPointer right, BinaryOperation operation)
        {
            if (left is Tuple leftTuple && right.Value is Tuple rightTuple)
            {
                if (leftTuple.Values.Count != rightTuple.Values.Count)
                    throw new Throw("Miss mathch number of elements inside the tuples");

                return new Tuple(leftTuple.Values
                    .Zip(rightTuple.Values, (a, b) => RecursivelyCompoundAssign(a, b, operation)).ToList());
            }

            if (left is Tuple tuple)
                return new Tuple(tuple.Values.Select(x => RecursivelyCompoundAssign(x, right, operation)).ToList());

            return CompoundAssign(left, right, operation);
        }

        private static IPointer Assign(IPointer left, IPointer right)
        {
            if (left is not Pointer pointer)
                throw new Throw("You cannot assign a value to a literal");

            var value = right.Value;

            return pointer.Set(value);
        }

        private static IPointer CompoundAssign(IPointer left, IPointer right, BinaryOperation operation)
        {
            if (left is not Pointer pointer)
                throw new Throw("You cannot assign a value to a literal");

            var value = RecursivelyCall(left, right, operation).Value;

            return pointer.Set(value);
        }
    }
}