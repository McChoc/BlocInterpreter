using Bloc.Variables;
using System;
using System.Collections.Generic;

namespace Bloc.Memory;

public class VariableCollection : IDisposable
{
    public Dictionary<string, Stack<StackVariable>> Variables { get; } = new();

    internal void Add(StackVariable variable)
    {
        if (Variables.TryGetValue(variable.Name, out var stack))
            stack.Push(variable);
        else
            Variables[variable.Name] = new(new[] { variable });
    }

    internal void Remove(string name)
    {
        Variables[name].Pop();

        if (Variables[name].Count == 0)
            Variables.Remove(name);
    }

    public virtual void Dispose()
    {
        foreach (var stack in Variables.Values)
            while (stack.Count > 0)
                stack.Peek().Delete();
    }
}