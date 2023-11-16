using System.Collections.Generic;
using System.Linq;
using Bloc.Core;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Values.Core;
using Bloc.Values.Types;
using Bloc.Variables;

namespace Bloc.Identifiers;

[Record]
internal sealed partial class TupleIdentifier : IIdentifier
{
    private readonly List<IIdentifier> _identifiers;

    public TupleIdentifier(List<IIdentifier> identifiers)
    {
        _identifiers = identifiers;
    }

    public Value From(Module module, Call call)
    {
        var values = _identifiers
            .Select(x => x.From(module, call))
            .ToList();

        return new Tuple(values);
    }

    public IValue Define(Value value, Call call, bool mask, bool mutable, VariableScope scope)
    {
        if (value is not Tuple tuple)
        {
            var values = _identifiers
                .Select(x => x.Define(value, call, mask, mutable, scope))
                .ToList();

            return new Tuple(values);
        }
        else
        {
            if (_identifiers.Count != tuple.Values.Count)
                throw new Throw("Miss match number of elements in tuples.");

            var values = _identifiers
                .Zip(tuple.Values, (a, b) => a.Define(b.Value, call, mask, mutable, scope))
                .ToList();

            return new Tuple(values);
        }
    }
}