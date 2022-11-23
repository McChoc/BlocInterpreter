using System.Collections.Generic;
using Bloc.Variables;

namespace Bloc.Memory
{
    public sealed class Scope
    {
        public Dictionary<string, StackVariable> Variables { get; } = new();

        internal Scope Copy()
        {
            var scope = new Scope();

            foreach (var (key, value) in Variables)
                scope.Variables[key] = new(key, value.Value.Copy(), scope);

            return scope;
        }

        internal void Destroy()
        {
            foreach (var variable in Variables.Values)
                variable.Delete();
        }
    }
}