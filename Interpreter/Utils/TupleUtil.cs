using System.Linq;
using Bloc.Results;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Utils
{
    internal delegate IValue UnaryOperation(IValue value);

    internal delegate IValue BinaryOperation(IValue left, IValue right);

    internal static class TupleUtil
    {
        internal static IValue RecursivelyCall(IValue value, UnaryOperation operation)
        {
            if (value.Value is Tuple tuple)
                return new Tuple(tuple.Values.Select(x => RecursivelyCall(x, operation)).ToList());

            return operation(value);
        }

        internal static IValue RecursivelyCall(IValue left, IValue right, BinaryOperation operation)
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

        internal static IValue RecursivelyAssign(IValue left, IValue right)
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

        internal static IValue Assign(IValue left, IValue right)
        {
            if (left is not Variable variable)
                throw new Throw("You cannot assign a value to a literal");

            var value = right.Value.Copy();

            value.Assign();
            variable.Value.Destroy();
            return variable.Value = value;
        }

        internal static IValue RecursivelyCompoundAssign(IValue left, IValue right, BinaryOperation operation)
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

        private static IValue CompoundAssign(IValue left, IValue right, BinaryOperation operation)
        {
            if (left is not Variable variable)
                throw new Throw("You cannot assign a value to a literal");

            var value = RecursivelyCall(left, right, operation);

            value.Value.Assign();
            variable.Value.Destroy();
            return variable.Value = value.Value;
        }
    }
}