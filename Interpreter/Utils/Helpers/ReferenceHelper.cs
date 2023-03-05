using System.Collections.Generic;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Utils.Helpers;

internal static class ReferenceHelper
{
    internal static IValue Resolve(IValue value, int hopLimit)
    {
        var hopCount = 0;

        return Resolve(value, ref hopCount, hopLimit);
    }

    private static IValue Resolve(IValue value, ref int hopCount, int hopLimit)
    {
        while (hopCount++ < hopLimit)
        {
            if (value.Value is not Reference reference)
                return value;

            value = reference.Pointer;
        }

        throw new Throw("The hop limit was reached");
    }

    internal static Value ResolveRecursive(Value value, int hopLimit)
    {
        return ResolveRecursive(value, 0, hopLimit);
    }

    private static Value ResolveRecursive(Value value, int hopCount, int hopLimit)
    {
        value = Resolve(value, ref hopCount, hopLimit).Value;

        if (value is Tuple tuple)
        {
            var values = new List<Value>(tuple.Values.Count);

            foreach (var item in tuple.Values)
                values.Add(ResolveRecursive(item.Value, hopCount, hopLimit));

            return new Tuple(values);
        }

        if (value is Struct @struct)
        {
            var values = new Dictionary<string, Value>(@struct.Values.Count);

            foreach (var (key, item) in @struct.Values)
                values[key] = ResolveRecursive(item.Value, hopCount, hopLimit);

            return new Struct(values);
        }

        if (value is Array array)
        {
            var values = new List<Value>(array.Values.Count);

            foreach (var item in array.Values)
                values.Add(ResolveRecursive(item.Value, hopCount, hopLimit));

            return new Array(values);
        }

        return value;
    }
}