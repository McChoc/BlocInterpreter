using System.Collections.Generic;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Utils
{
    internal static class ReferenceUtil
    {
        internal static IPointer Dereference(IPointer value, int hopLimit)
        {
            var hopCount = 0;

            return Dereference(value, ref hopCount, hopLimit);
        }

        private static IPointer Dereference(IPointer value, ref int hopCount, int hopLimit)
        {
            for (; hopCount < hopLimit; hopCount++)
            {
                if (!value.Value.Is(out Reference? reference))
                    return value;

                value = reference!.Pointer;
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
                var values = new List<IPointer>(tuple.Values.Count);

                foreach (var item in tuple.Values)
                    values.Add(TrueValue(item.Value, hopCount, hopLimit));

                return new Tuple(values);
            }

            if (value is Struct @struct)
            {
                var values = new Dictionary<string, IVariable>(@struct.Values.Count);

                foreach (var (key, item) in @struct.Values)
                    values[key] = TrueValue(item.Value, hopCount, hopLimit);

                return new Struct(values);
            }

            if (value is Array array)
            {
                var values = new List<IVariable>(array.Values.Count);

                foreach (var item in array.Values)
                    values.Add(TrueValue(item.Value, hopCount, hopLimit));

                return new Array(values);
            }

            return value;
        }
    }
}