using System.Linq;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Utils.Helpers;

internal static class VariableHelper
{
    internal static IValue Define(IValue identifier, Value value, Call call, bool mask = false, bool mutable = true)
    {
        if (identifier is UnresolvedPointer pointer)
            return pointer.Define(mask, mutable, value, call);

        if (identifier is Tuple tuple && value is not Tuple)
            return new Tuple(tuple.Values
                .Select(x => Define(x, value, call, mask, mutable))
                .ToList());

        if (identifier is Tuple leftTuple && value is Tuple rightTuple)
        {
            if (leftTuple.Values.Count != rightTuple.Values.Count)
                throw new Throw("Miss match number of elements in tuples.");

            var values = leftTuple.Values
                .Zip(rightTuple.Values, (a, b) => Define(a, b.Value, call, mask, mutable))
                .ToList();

            return new Tuple(values);
        }

        throw new Throw("Expected an identifier");
    }
}