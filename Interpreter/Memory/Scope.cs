using System;
using Bloc.Utils.Attributes;

namespace Bloc.Memory;

[Record]
public sealed partial class Scope : VariableCollection, IDisposable
{
    private readonly Call _call;

    public Scope(Call call)
    {
        _call = call;
        _call.Scopes.Add(this);
    }

    public void Dispose()
    {
        foreach (var stack in Variables.Values)
            while (stack.Count > 0)
                stack.Peek().Delete();

        _call.Scopes.RemoveAt(_call.Scopes.Count - 1);
    }
}