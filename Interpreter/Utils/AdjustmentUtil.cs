using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Utils
{
    internal delegate (Value, Value) Adjustment(Value value);

    internal static class AdjustmentUtil
    {
        internal static Value Adjust(IValue value, Adjustment adjustment, Call call)
        {
            value = ReferenceUtil.Dereference(value, call.Engine.HopLimit);

            if (value.Value is Tuple tuple)
            {
                var values = new List<Value>(tuple.Variables.Count);

                foreach (var item in tuple.Variables)
                    values.Add(Adjust(item, adjustment, call));

                return new Tuple(values);
            }

            if (value is Pointer pointer)
            {
                var (returned, actual) = Evaluate(pointer.Get(), adjustment, call);

                pointer.Set(actual);

                return returned;
            }

            throw new Throw("The operand must be a variable");
        }

        private static (Value, Value) Evaluate(Value value, Adjustment adjustment, Call call)
        {
            if (value is Reference reference)
                return (Adjust(reference, adjustment, call), reference);

            if (value is Tuple tuple)
            {
                var returnedValues = new List<Value>(tuple.Variables.Count);
                var actualValues = new List<Value>(tuple.Variables.Count);

                foreach (var item in tuple.Variables)
                {
                    var (returned, actual) = Evaluate(item.Value, adjustment, call);

                    returnedValues.Add(returned);
                    actualValues.Add(actual);
                }

                return (new Tuple(returnedValues), new Tuple(actualValues));
            }

            return adjustment(value);
        }
    }
}