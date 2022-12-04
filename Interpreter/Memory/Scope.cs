using System.Collections.Generic;
using Bloc.Variables;

namespace Bloc.Memory
{
    public sealed class Scope
    {
        public Dictionary<string, StackVariable> Variables { get; } = new();

        internal void Destroy()
        {
            foreach (var variable in Variables.Values)
                variable.Delete();
        }
    }
}