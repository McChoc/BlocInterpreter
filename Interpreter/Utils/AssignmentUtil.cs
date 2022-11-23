using System.Collections.Generic;
using System.Linq;
using Bloc.Extensions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Utils
{
    internal static class AssignmentUtil
    {
        internal static Value Assign(IPointer left, IPointer right, Call call)
        {
            if (left is Tuple leftTuple && right.Value is Tuple rightTuple)
            {
                if (leftTuple.Values.Count != rightTuple.Values.Count)
                    throw new Throw("Miss mathch number of elements inside the tuples");

                return new Tuple(leftTuple.Values.Zip(rightTuple.Values, (a, b) => Assign(a, b, call)).ToList<IPointer>());
            }

            if (left is Tuple tuple)
                return new Tuple(tuple.Values.Select(x => Assign(x, right, call)).ToList<IPointer>());

            if (left is Pointer pointer)
                return pointer.Set(right.Value);

            throw new Throw("You cannot assign a value to a literal");
        }

        internal static Value CompoundAssign(IPointer left, IPointer right, BinaryOperation operation, Call call)
        {
            left = ReferenceUtil.Dereference(left, call.Engine.HopLimit);
            right = ReferenceUtil.Dereference(right, call.Engine.HopLimit);

            if (left is Tuple tuple)
            {
                var values = new List<IPointer>(tuple!.Values.Count);

                if (right.Value is Tuple rightTuple)
                {
                    if (tuple.Values.Count != rightTuple.Values.Count)
                        throw new Throw("Miss mathch number of elements inside the tuples");

                    foreach (var (a, b) in tuple.Values.Zip(rightTuple.Values))
                        values.Add(CompoundAssign(a, b, operation, call));
                }
                else
                {
                    foreach (var item in tuple.Values)
                        values.Add(CompoundAssign(item, right, operation, call));
                }

                return new Tuple(values);
            }

            if (left is Pointer pointer)
            {
                var (returned, actual) = Evaluate(left.Value, right.Value, operation, call);

                pointer.Set(actual);

                return returned;
            }

            throw new Throw("The operand must be a variable");
        }

        private static (Value, Value) Evaluate(Value left, Value right, BinaryOperation assignment, Call call)
        {
            if (left is Reference reference)
                return (CompoundAssign(reference, right, assignment, call), reference);

            int count;

            IEnumerable<IPointer> leftEnumerable, rightEnumerable;

            switch (left, right)
            {
                case (Tuple leftTuple, Tuple rightTuple):
                    if (leftTuple.Values.Count != rightTuple.Values.Count)
                        throw new Throw("Miss mathch number of elements inside the tuples");

                    count = leftTuple.Values.Count;
                    leftEnumerable = leftTuple.Values;
                    rightEnumerable = rightTuple.Values;
                    break;

                case (Tuple leftTuple, _):
                    count = leftTuple.Values.Count;
                    leftEnumerable = leftTuple.Values;
                    rightEnumerable = Enumerable.Repeat(right, count);
                    break;

                case (_, Tuple rightTuple):
                    count = rightTuple.Values.Count;
                    leftEnumerable = Enumerable.Repeat(left, count);
                    rightEnumerable = rightTuple.Values;
                    break;

                default:
                    var value = assignment(left, right);
                    return (value, value);
            }

            var returnedValues = new List<IPointer>(count);
            var actualValues = new List<IPointer>(count);

            foreach (var (a, b) in leftEnumerable.Zip(rightEnumerable))
            {
                var (returned, actual) = Evaluate(a.Value, b.Value, assignment, call);

                returnedValues.Add(returned);
                actualValues.Add(actual);
            }

            return (new Tuple(returnedValues), new Tuple(actualValues));
        }
    }
}