using System.Collections.Generic;
using Bloc.Variables;

namespace Bloc.Memory
{
    internal class Scope
    {
        internal Scope(Call? call)
        {
            Call = call;
        }

        internal Call? Call { get; }

        internal Dictionary<string, Variable> Variables { get; } = new();

        internal void Destroy()
        {
            foreach (var variable in Variables.Values)
                variable.Destroy();
        }
    }
}