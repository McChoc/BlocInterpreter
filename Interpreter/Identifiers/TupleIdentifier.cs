using System.Collections.Generic;
using System.Linq;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values.Core;
using Tuple = Bloc.Values.Types.Tuple;

namespace Bloc.Identifiers;

internal sealed class TupleIdentifier : IIdentifier
{
    private readonly List<IIdentifier> _identifiers;

    public TupleIdentifier(List<IIdentifier> identifiers)
    {
        _identifiers = identifiers;
    }

    public IValue Define(Value value, Call call, bool mask = false, bool mutable = true)
    {
        if (value is not Tuple tuple)
        {
            var values = _identifiers
                .Select(x => x.Define(value, call, mask, mutable))
                .ToList();

            return new Tuple(values);
        }
        else
        {
            if (_identifiers.Count != tuple.Values.Count)
                throw new Throw("Miss match number of elements in tuples.");

            var values = _identifiers
                .Zip(tuple.Values, (a, b) => a.Define(b.Value, call, mask, mutable))
                .ToList();

            return new Tuple(values);
        }
    }
}