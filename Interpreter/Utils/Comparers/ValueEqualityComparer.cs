using System;
using System.Collections.Generic;
using Bloc.Values.Core;

namespace Bloc.Utils.Comparers;

internal sealed class ValueEqualityComparer : IEqualityComparer<IValue>
{
    public int GetHashCode(IValue obj)
    {
        return HashCode.Combine(obj.Value);
    }

    public bool Equals(IValue x, IValue y)
    {
        return object.Equals(x.Value, y.Value);
    }
}