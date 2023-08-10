using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Utils.Comparers;

internal sealed class FuncBasedValueComparer : IComparer<Value>
{
    private readonly Func _func;
    private readonly Call _call;

    public FuncBasedValueComparer(Func func, Call call)
    {
        _func = func;
        _call = call;
    }

    public int Compare(Value x, Value y)
    {
        var value = _func.Invoke(new() { x.Copy(), y.Copy() }, new(), _call);

        return Number.ImplicitCast(value).GetInt();
    }
}