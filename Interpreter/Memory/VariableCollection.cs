using System.Collections.Generic;
using Bloc.Utils.Attributes;
using Bloc.Variables;

namespace Bloc.Memory;

[Record]
public partial class VariableCollection
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
}