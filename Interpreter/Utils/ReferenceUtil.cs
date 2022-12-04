using System.Collections.Generic;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Utils
{
    internal static class ReferenceUtil
    {
        internal static IValue Dereference(IValue value, int hopLimit)
        {
            var hopCount = 0;

            return Dereference(value, ref hopCount, hopLimit);
        }

        private static IValue Dereference(IValue value, ref int hopCount, int hopLimit)
        {
            while (hopCount++ < hopLimit)
            {
                if (value.Value is not Reference reference)
                    return value;

                value = reference.Pointer;
            }

            throw new Throw("The hop limit was reached");
        }

        internal static Value TrueValue(Value value, int hopLimit)
        {
            return TrueValue(value, 0, hopLimit);
        }

        private static Value TrueValue(Value value, int hopCount, int hopLimit)
        {
            value = Dereference(value, ref hopCount, hopLimit).Value;

            if (value is Tuple tuple)
            {
                var values = new List<Value>(tuple.Variables.Count);

                foreach (var item in tuple.Variables)
                    values.Add(TrueValue(item.Value, hopCount, hopLimit));

                return new Tuple(values);
            }

            if (value is Struct @struct)
            {
                var values = new Dictionary<string, Value>(@struct.Variables.Count);

                foreach (var (key, item) in @struct.Variables)
                    values[key] = TrueValue(item.Value, hopCount, hopLimit);

                return new Struct(values);
            }

            if (value is Array array)
            {
                var values = new List<Value>(array.Variables.Count);

                foreach (var item in array.Variables)
                    values.Add(TrueValue(item.Value, hopCount, hopLimit));

                return new Array(values);
            }

            return value;
        }
    }
}