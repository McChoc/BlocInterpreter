using System.Collections.Generic;
using Bloc.Variables;

namespace Bloc.Memory
{
    public sealed class Scope
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

        internal void Dispose()
        {
            foreach (var stack in Variables.Values)
                while (stack.Count > 0)
                    stack.Peek().Delete();
        }
    }
}