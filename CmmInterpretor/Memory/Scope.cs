using CmmInterpretor.Variables;
using System.Collections.Generic;

namespace CmmInterpretor.Memory
{
    internal class Scope
    {
        internal Call? Call { get; }

        internal Dictionary<string, Variable> Variables { get; } = new();

        internal Scope(Call? call) => Call = call;

        internal void Destroy()
        {
            foreach (var variable in Variables.Values)
                variable.Destroy();
        }
    }
}
