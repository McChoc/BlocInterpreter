using System;

namespace Bloc.Memory;

public sealed class Scope : VariableCollection, IDisposable
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

        _call.Scopes.Remove(this);
    }
}